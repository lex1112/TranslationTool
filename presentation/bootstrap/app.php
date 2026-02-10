<?php

use Illuminate\Foundation\Configuration\Middleware;

use Illuminate\Foundation\Application;
use Illuminate\Foundation\Configuration\Exceptions;
use Illuminate\Http\Request;
use Symfony\Component\HttpKernel\Exception\NotFoundHttpException;
use Illuminate\Http\Client\ConnectionException;

return Application::configure(basePath: dirname(__DIR__))
    ->withRouting(
        web: __DIR__.'/../routes/web.php',
        commands: __DIR__.'/../routes/console.php',
        health: '/up',
    )
    ->withMiddleware(function (Middleware $middleware) {
        $middleware->encryptCookies(except: [
            '.AspNetCore.Identity.Application',
        ]);
    })
    ->withMiddleware(function (Middleware $middleware) {
        $middleware->alias([
            'auth.oidc' => \App\Http\Middleware\CheckOidcAuth::class,
        ]);
    })
    ->withExceptions(function (Exceptions $exceptions) {
        
        // Catch 404 Errors (Resource Not Found)
        $exceptions->render(function (NotFoundHttpException $e, Request $request) {
            return response()->view('errors.404', ['message' => 'Page or SID not found'], 404);
        });

        // Catch .NET Connection Errors (Backend Offline)
        $exceptions->render(function (ConnectionException $e, Request $request) {
            return response()->view('errors.500', ['message' => 'The .NET Backend is currently offline. Check your Docker containers.'], 500);
        });

        // Catch General HTTP Failures from the .NET Backend
        $exceptions->render(function (\Exception $e, Request $request) {
            // Log for your own debugging
            \Log::error($e->getMessage());

            // Return back with the exact error message from the backend
            return back()->withErrors($e->getMessage());
        });

    })->create();

