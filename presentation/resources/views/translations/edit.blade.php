<!-- edit.blade.php -->
<h1>Edit SID: {{ $details['sid'] }}</h1>

<p>
    <strong>System-default (en-US):</strong><br>
    <input type="text" value="{{ $defaultText }}" readonly style="width: 100%; background: #eee;">
</p>

<!-- Language Selector (Refreshes page to load specific translation) -->
<form method="GET" action="{{ route('edit', $details['sid']) }}">
    <label>Select language to edit:</label>
    <select name="edit_lang" onchange="this.form.submit()">
        <option value="en-US" {{ $editLang == 'en-US' ? 'selected' : '' }}>en-US</option>
        <option value="de-DE" {{ $editLang == 'de-DE' ? 'selected' : '' }}>de-DE</option>
        <option value="fr-FR" {{ $editLang == 'fr-FR' ? 'selected' : '' }}>fr-FR</option>
    </select>
</form>

<hr>

<!-- Update Form -->
<form action="{{ route('update', $details['sid']) }}" method="POST">
    @csrf
    <!-- IMPORTANT: If your .NET API expects PUT, keep this. If POST, remove it. -->
    @method('POST') 
    
    <input type="hidden" name="langId" value="{{ $editLang }}">

    <div>
        <label>Translation Text:</label><br>
        <textarea name="text" rows="5" style="width: 100%">{{ $currentText }}</textarea>
    </div>

    <div style="margin-top: 10px;">
        <button type="submit" style="background: green; color: white;">Apply/Save Changes</button>
        <a href="{{ route('index') }}">Discard and Return</a>
    </div>
</form>

<hr>

<!-- Delete Form -->
<form action="{{ route('delete', $details['sid']) }}" method="POST" onsubmit="return confirm('Are you sure you want to delete this SID and ALL translations?')">
    @csrf
    @method('DELETE')
    <button type="submit" style="background: red; color: white;">Delete the SID</button>
</form>

