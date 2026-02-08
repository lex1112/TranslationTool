<?php
namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;

class CheckOidcAuth
{
    public function handle(Request $request, Closure $next)
    {
        if (!session()->has('api_token')) {
            return redirect()->route('login');
        }

        return $next($request);
    }
}
