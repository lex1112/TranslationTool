<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class LoginController extends Controller
{
    public function showLoginForm()
    {
        return view('auth.login');
    }

    public function login(Request $request)
    {
        $credentials = $request->validate([
            'username' => 'required|string',
            'password' => 'required|string',
        ]);

        $internalUrl = config('services.backend.internal_url');
        $browserUrl = config('services.backend.browser_url');

        // Call the .NET Login API via Internal Network (Docker)
        $response = Http::post("{$internalUrl}/api/account/login", [
            'username' => $credentials['username'],
            'password' => $credentials['password'],
        ]);

        if ($response->successful()) {
            // Prepare OIDC Authorize URL using Browser-accessible URL
            $queryParams = http_build_query([
                'client_id' => config('services.backend.client_id'),
                'redirect_uri' => config('services.backend.redirect_uri'),
                'response_type' => 'code',
                'scope' => 'openid profile email', 
            ]);

            $authorizeUrl = "{$browserUrl}/connect/authorize?{$queryParams}";

            // Forward the .NET Identity Cookie to the user's browser
            $rawCookie = $response->header('Set-Cookie');
            $redirect = redirect()->away($authorizeUrl);

            return $rawCookie ? $redirect->withHeaders(['Set-Cookie' => $rawCookie]) : $redirect;
        }

        return back()->withErrors(['message' => 'Invalid credentials from .NET Backend']);
    }

    public function callback(Request $request)
    {
        $code = $request->query('code');
        $internalUrl = config('services.backend.internal_url');

        // Exchange the Code for a JWT Token via Internal Network
        $response = Http::asForm()->post("{$internalUrl}/connect/token", [
            'grant_type' => 'authorization_code',
            'client_id' => config('services.backend.client_id'),
            'client_secret' => config('services.backend.client_secret'),
            'code' => $code,
            'redirect_uri' => config('services.backend.redirect_uri'),
        ]);

        if ($response->successful()) {
            $token = $response->json()['access_token'];

            // Save token to session for the api() helper in TranslationController
            session(['api_token' => $token]);

            return redirect()->route('index');
        }

        return redirect()->route('login')->withErrors('Authentication failed during token exchange.');
    }
}
