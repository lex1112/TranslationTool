<!DOCTYPE html>
<html>
<head>
    <title>500 - Server Error</title>
    <style>
        body { font-family: sans-serif; text-align: center; padding: 50px; background: #f4f4f4; }
        .box { background: white; padding: 30px; display: inline-block; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #636b6f; font-size: 50px; margin: 0; }
        .error-msg { color: #721c24; background: #f8d7da; padding: 10px; margin-top: 15px; border-radius: 4px; }
        .btn { display: inline-block; margin-top: 20px; padding: 10px 20px; background: #3490dc; color: white; text-decoration: none; border-radius: 4px; }
    </style>
</head>
<body>
    <div class="box">
        <h1>500</h1>
        <p>Something went wrong with the Translation Service.</p>
        
        <div class="error-msg">
            <strong>Details:</strong> {{ $exception->getMessage() ?: "Connection failed or Backend crashed." }}
        </div>

        <a href="{{ route('index') }}" class="btn">Try to Reload</a>
    </div>
</body>
</html>
