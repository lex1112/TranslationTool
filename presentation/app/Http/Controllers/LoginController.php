<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class LoginController extends Controller
{
    // Show the PHP Login Form
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

        // 1. Call the .NET Login API (JSON)
        // Use host.docker.internal if .NET is on Windows, or 'app' if in Docker
        $response = Http::post('http://backend:8080/api/account/login', [
            'username' => $credentials['username'],
            'password' => $credentials['password'],
        ]);

        if ($response->successful()) {
            // 2. Prepare the OIDC Authorize URL
            $queryParams = http_build_query([
                'client_id' => 'php-client',
                'redirect_uri' => 'http://localhost:8000/login/callback',
                'response_type' => 'code',
                'scope' => 'openid profile email',
            ]);

            $authorizeUrl = 'http://localhost:8080/connect/authorize?' . $queryParams;

            // 3. REDIRECT the user to .NET Authorize
            // .NET will see the cookie set during the API call because both are on localhost

            $rawCookie = $response->header('Set-Cookie');

            $redirect = redirect()->away($authorizeUrl);

            if ($rawCookie) {
                // We "forward" the header exactly as .NET sent it
                return $redirect->withHeaders([
                    'Set-Cookie' => $rawCookie
                ]);
            }

            return $redirect;
        }

        return back()->withErrors(['message' => 'Invalid credentials from .NET Backend']);
    }

    public function callback(Request $request)
    {
        $code = $request->query('code');

        // Exchange the Code for a JWT Token
        $response = Http::asForm()->post(env('BACKEND_INTERNAL_URL') . '/connect/token', [
            'grant_type' => 'authorization_code',
            'client_id' => 'php-client',
            'client_secret' => 'secret-123', 
            'code' => $code,
            'redirect_uri' => 'http://localhost:8000/login/callback',
        ]);

        if ($response->successful()) {
            $token = $response->json()['access_token'];

            // Save token to session for the api() helper
            session(['api_token' => $token]);

            // SUCCESS: Redirect to the Translations Index
            return redirect()->route('index');
        }

        if ($response->failed()) {
            dd([
                'status' => $response->status(),
                'error_body' => $response->json(),
                'sent_payload' => [
                    'code' => $code,
                    'redirect_uri' => 'http://localhost:8000',
                ]
            ]);
        }

        return redirect('/login')->withErrors('Authentication failed.');
    }
}
