<?php
namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;

class CheckOidcAuth
{
    public function handle($request, Closure $next)
    {
        // List of routes that DO NOT need a token
        $excludedRoutes = [
            '',
            'login',
            'login/callback',
            'api/account/login'
        ];

        if ($request->is($excludedRoutes)) {
            return $next($request);
        }

        // Check for token
        if (!session()->has('api_token')) {
            return redirect()->route('login');
        }

        return $next($request);
    }

}
