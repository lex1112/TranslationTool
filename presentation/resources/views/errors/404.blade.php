<!DOCTYPE html>
<html>
<head>
    <title>404 - Not Found</title>
    <style>
        body { font-family: sans-serif; text-align: center; padding: 50px; background: #f4f4f4; }
        .box { background: white; padding: 30px; display: inline-block; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #e3342f; font-size: 50px; margin: 0; }
        .btn { display: inline-block; margin-top: 20px; padding: 10px 20px; background: #3490dc; color: white; text-decoration: none; border-radius: 4px; }
    </style>
</head>
<body>
    <div class="box">
        <h1>404</h1>
        <p>{{ $exception->getMessage() ?: "The requested resource does not exist." }}</p>
        <a href="{{ route('index') }}" class="btn">Back to List</a>
    </div>
</body>
</html>
