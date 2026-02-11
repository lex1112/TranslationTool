<?php

return [

    /*
    |--------------------------------------------------------------------------
    | Third Party Services
    |--------------------------------------------------------------------------
    |
    | This file is for storing the credentials for third party services such
    | as Mailgun, Postmark, AWS and more. This file provides the de facto
    | location for this type of information, allowing packages to have
    | a conventional file to locate the various service credentials.
    |
    */

    'postmark' => [
        'key' => env('POSTMARK_API_KEY'),
    ],

    'resend' => [
        'key' => env('RESEND_API_KEY'),
    ],

    'ses' => [
        'key' => env('AWS_ACCESS_KEY_ID'),
        'secret' => env('AWS_SECRET_ACCESS_KEY'),
        'region' => env('AWS_DEFAULT_REGION', 'us-east-1'),
    ],

    'slack' => [
        'notifications' => [
            'bot_user_oauth_token' => env('SLACK_BOT_USER_OAUTH_TOKEN'),
            'channel' => env('SLACK_BOT_USER_DEFAULT_CHANNEL'),
        ],
    ],

    'backend' => [
        'internal_url'  => env('BACKEND_INTERNAL_URL', 'http://dotnet_backend:8080'),
        'browser_url'   => env('BACKEND_BROWSER_URL', 'http://localhost:8080'),
        'client_id'     => env('OIDC_CLIENT_ID', 'php-client'),
        'client_secret' => env('OIDC_CLIENT_SECRET', 'secret-123'),
        'redirect_uri'  => env('OIDC_REDIRECT_URI', 'http://localhost:8000/login/callback'),
    ],
];

