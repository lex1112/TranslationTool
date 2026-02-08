<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class TranslationController extends Controller
{
    private function api()
    {
        return Http::withToken(session('api_token'))->baseUrl(env('BACKEND_INTERNAL_URL'));
    }

    public function index(Request $request)
    {
        $selectedLang = $request->get('lang', 'en-US');

        // 1. Get all SIDs from .Net

        $response = $this->api()->get('/api/translations');

        $sids =$response->json();

        $tableData = [];
 
        foreach (($sids ?? []) as $sid) {
            // 2. Get details for each SID
            $details = $this->api()->get("/api/translations/{$sid}")->json();
            $translations = collect($details['translations']);

            // 3. Logic: Target Lang -> Fallback to en-US -> Fallback to 'default'
            $text = $translations->where('langId', $selectedLang)->first()['text']
                ?? $translations->where('langId', 'en-US')->first()['text']
                ?? $translations->where('langId', 'default')->first()['text']
                ?? '---';

            $tableData[] = ['sid' => $sid, 'text' => $text];
        }

        return view('translations.index', compact('tableData', 'selectedLang'));
    }

    public function edit($sid, Request $request)
    {
        $details = $this->api()->get("/api/translations/{$sid}")->json();
        $editLang = $request->get('edit_lang', 'en-US');

        $defaultText = collect($details['translations'])->where('langId', 'en-US')->first()['text'] ?? 'N/A';
        $currentText = collect($details['translations'])->where('langId', $editLang)->first()['text'] ?? '';

        return view('translations.edit', compact('details', 'defaultText', 'currentText', 'editLang'));
    }

    public function update(Request $request, $sid)
    {
        $this->api()->put("/api/translations/{$sid}/{$request->langId}", $request->text);
        return redirect()->route('index');
    }

    public function destroy($sid)
    {
        $this->api()->delete("/api/translations/{$sid}");
        return redirect()->route('index');
    }
}

