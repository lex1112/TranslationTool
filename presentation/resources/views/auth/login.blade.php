<!DOCTYPE html>
<html>
<head>
    <title>Login</title>
    <style>
        body { font-family: sans-serif; display: flex; justify-content: center; padding-top: 50px; }
        .card { width: 300px; border: 1px solid #ccc; padding: 20px; border-radius: 8px; }
        input { width: 100%; margin-bottom: 10px; padding: 8px; box-sizing: border-box; }
        button { width: 100%; padding: 10px; background: #007bff; color: white; border: none; cursor: pointer; }
    </style>
</head>
<body>
    <div class="card">
        <h3>Login to System</h3>
        <form method="POST" action="/login">
            @csrf
            <input type="text" name="username" placeholder="Email/Username" required>
            <input type="password" name="password" placeholder="Password" required>
            <button type="submit">Sign In</button>
        </form>
        @if ($errors->any())
            <p style="color: red;">{{ $errors->first() }}</p>
        @endif
    </div>
</body>
</html>
