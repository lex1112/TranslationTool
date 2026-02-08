<form method="GET" id="langForm">
    <select name="lang" onchange="document.getElementById('langForm').submit()">
        <option value="en-US" {{ $selectedLang == 'en-US' ? 'selected' : '' }}>English</option>
        <option value="de-DE" {{ $selectedLang == 'de-DE' ? 'selected' : '' }}>German</option>
    </select>
</form>

<table border="1" width="100%">
    <thead>
        <tr><th>SID</th><th>Text ({{ $selectedLang }})</th></tr>
    </thead>
    <tbody>
        @foreach($tableData as $row)
        <tr ondblclick="window.location='{{ route('edit', $row['sid']) }}'" style="cursor:pointer">
            <td>{{ $row['sid'] }}</td>
            <td>{{ $row['text'] }}</td>
        </tr>
        @endforeach
    </tbody>
</table>

