<!-- resources/views/translations/edit.blade.php -->

<h1>Edit SID: {{ $details['sid'] }}</h1>

<!-- 1. System Default Display -->
<p>
    <strong>System-default (en-US):</strong><br>
    <input type="text" value="{{ $defaultText }}" readonly style="width: 100%; background: #eee;">
</p>

<!-- 2. Language Selector -->
<form method="GET" action="{{ route('edit', $details['sid']) }}">
    <label>Select language to edit:</label>
    <select name="edit_lang" onchange="this.form.submit()">
        @foreach($availableLanguages as $langId)
            <option value="{{ $langId }}" {{ $editLang == $langId ? 'selected' : '' }}>
                {{ $langId }}
            </option>
        @endforeach
    </select>
</form>

<hr>

<!-- 3. Update Form -->
<form action="{{ route('update', $details['sid']) }}" method="POST">
    @csrf
    <!-- langId tells .NET which translation to update -->
    <input type="hidden" name="langId" value="{{ $editLang }}">

    <div>
        <label>Translation Text ({{ $editLang }}):</label><br>
        <textarea name="text" rows="5" style="width: 100%">{{ $currentText }}</textarea>
    </div>

    <div style="margin-top: 10px;">
        <button type="submit" style="background: green; color: white;">Apply/Save Changes</button>
        <a href="{{ route('index') }}">Discard and Return</a>
    </div>
</form>

<hr>

<!-- 4. Delete SID (and all translations) -->
<form action="{{ route('delete', $details['sid']) }}" method="POST" onsubmit="return confirm('Delete this SID?')">
    @csrf
    @method('DELETE')
    <button type="submit" style="background: red; color: white;">Delete SID</button>
</form>
