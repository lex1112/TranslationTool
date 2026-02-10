<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class TranslationController extends Controller
{
    /**
     * Creates a pre-configured HTTP client with the Bearer token.
     * Redirects to login if the token is missing from the session.
     */
    private function api()
    {
        $token = session('api_token');

        if (!$token) {
            // Immediate redirect if no token exists in the current session
            abort(redirect()->route('login')->withErrors('Unauthorized. Please login again.'));
        }

        return Http::withToken($token)
            ->baseUrl(env('BACKEND_INTERNAL_URL'))
            ->timeout(10) // Prevents the script from hanging if .NET is unresponsive
            ->acceptJson();
    }

    /**
     * Centralized request wrapper to handle API responses and Token validation.
     * Detects 401 (Expired Token) and handles server-side errors.
     */
    private function request($method, $url, $data = [])
    {
        try {
            // 1. Execute the request via the api() helper
            $response = $this->api()->$method($url, $data);

            // 2. ALWAYS check for 401 (Expired or Invalid Token)
            if ($response->status() === 401) {
                // Clear the stale token from Laravel session
                session()->forget('api_token');

                // Force redirect to login with a clear message
                abort(redirect()->route('login')->withErrors('Your backend session has expired. Please log in again.'));
            }

            // 3. Check for other backend failures (500, 403, 404)
            if ($response->failed()) {
                // Extract error message from .NET JSON or use a default
                $error = $response->json()['message'] ?? 'The Translation Service returned an error.';
                throw new \Exception($error);
            }

            return $response->json();

        } catch (\Illuminate\Http\Client\ConnectionException $e) {
            // Handle cases where the .NET Docker container is down
            throw new \Exception('Unable to connect to the Translation Service. Is the backend running?');
        }
    }


    public function index(Request $request)
    {
        try {
            $selectedLang = $request->get('lang', 'en-US');

            // Fetch detailed resources from the .NET API
            $resources = $this->request('get', '/api/translations');

            // Extract unique language IDs for the filter dropdown
            $availableLanguages = collect($resources)
                ->pluck('translations')
                ->flatten(1)
                ->pluck('langId')
                ->unique()
                ->values();

            // Format data for the view with fallback logic
            $tableData = collect($resources)->map(function ($item) use ($selectedLang) {
                $translations = collect($item['translations']);

                // Logic: Target Lang -> Fallback en-US -> Fallback string
                $text = $translations->where('langId', $selectedLang)->first()['text']
                    ?? $translations->where('langId', 'en-US')->first()['text']
                    ?? '---';

                return [
                    'sid' => $item['sid'],
                    'text' => $text
                ];
            });

            return view('translations.index', compact('tableData', 'selectedLang', 'availableLanguages'));
        } catch (\Exception $e) {
            return back()->withErrors($e->getMessage());
        }
    }

    public function edit($sid, Request $request)
    {
        try {
            $editLang = $request->get('edit_lang', 'en-US');

            // Fetch specific resource and all translations (to get lang list)
            $details = $this->request('get', "/api/translations/{$sid}");
            $allResources = $this->request('get', '/api/translations');

            $availableLanguages = collect($allResources)
                ->pluck('translations')
                ->flatten(1)
                ->pluck('langId')
                ->unique()
                ->values();

            $translations = collect($details['translations'] ?? []);
            $defaultText = $translations->where('langId', 'en-US')->first()['text'] ?? 'N/A';
            $currentText = $translations->where('langId', $editLang)->first()['text'] ?? '';

            return view('translations.edit', compact(
                'details',
                'defaultText',
                'currentText',
                'editLang',
                'availableLanguages'
            ));
        } catch (\Exception $e) {
            return redirect()->route('index')->withErrors($e->getMessage());
        }
    }

    public function update(Request $request, $sid)
    {
        $request->validate([
            'langId' => 'required|string',
            'text' => 'required|string',
        ]);

        try {
            // Update the translation in the .NET database
            $this->request('put', "/api/translations/{$sid}/{$request->langId}", [
                'text' => $request->text,
            ]);

            return redirect()->route('index')->with('success', 'Translation updated successfully');
        } catch (\Exception $e) {
            return back()->withInput()->withErrors($e->getMessage());
        }
    }

    public function create()
    {
        return view('translations.create');
    }

    public function store(Request $request)
    {
        // 1. Validate the form input
        $request->validate([
            'sid' => 'required|string|max:255',
            'defaultText' => 'required|string',
        ]);

        try {
            // 2. Call the .NET POST /api/translations endpoint
            // This matches your .NET CreateRequest { sid, defaultText }
            $this->request('post', '/api/translations', [
                'sid' => $request->sid,
                'defaultText' => $request->defaultText,
            ]);

            return redirect()->route('index')->with('success', 'New SID created successfully');
        } catch (\Exception $e) {
            // If .NET returns a 409 Conflict (SID exists), this catches it
            return back()->withInput()->withErrors($e->getMessage());
        }
    }

    public function destroy($sid)
    {
        try {
            $this->request('delete', "/api/translations/{$sid}");
            return redirect()->route('index')->with('success', 'Resource deleted successfully');
        } catch (\Exception $e) {
            return back()->withErrors($e->getMessage());
        }
    }
}



