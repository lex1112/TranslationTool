<h1>Create New Translation Resource (SID)</h1>

<form action="{{ route('translations.store') }}" method="POST">
    @csrf
    <div>
        <label>SID Identifier (e.g. LOGIN_BUTTON):</label><br>
        <input type="text" name="sid" value="{{ old('sid') }}" required>
    </div>

    <br>

    <div>
        <label>Default Text (en-US):</label><br>
        <textarea name="defaultText" rows="3" required>{{ old('defaultText') }}</textarea>
    </div>

    <br>

    <button type="submit" style="background: green; color: white;">Create SID</button>
    <a href="{{ route('index') }}">Cancel</a>
</form>

@if($errors->any())
    <div style="color: red; margin-top: 20px;">
        {{ $errors->first() }}
    </div>
@endif
