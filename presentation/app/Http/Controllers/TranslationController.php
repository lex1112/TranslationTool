<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class TranslationController extends Controller
{
    private function api()
    {
        $token = session('api_token');

        if (!$token) {
            abort(redirect()->route('login')->withErrors('Unauthorized. Please login again.'));
        }

        // Use config() instead of env() for better performance and reliability
        $baseUrl = config('services.backend.internal_url');

        return Http::withToken($token)
            ->baseUrl($baseUrl)
            ->timeout(10)
            ->acceptJson();
    }

    /**
     * Centralized request wrapper.
     */
    private function request($method, $url, $data = [])
    {
        try {
            $response = $this->api()->$method($url, $data);

            if ($response->status() === 401) {
                session()->forget('api_token');
                abort(redirect()->route('login')->withErrors('Your session has expired.'));
            }

            if ($response->failed()) {
                // Log detailed error for the developer
                Log::warning("API Error at {$url}: " . $response->body());

                $error = $response->json()['message'] ?? 'Backend server error';
                throw new \Exception($error);
            }

            return $response->json();

        } catch (\Illuminate\Http\Client\ConnectionException $e) {
            Log::error("Connection failed to .NET Backend: " . $e->getMessage());
            throw new \Exception('Translation Service is unreachable. Check Docker containers.');
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



