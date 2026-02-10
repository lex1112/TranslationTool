<div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
    
    <!-- 1. Dynamic Language Selector -->
    <form method="GET" id="langForm" style="margin: 0;">
        <label for="lang">View Language:</label>
        <select name="lang" onchange="document.getElementById('langForm').submit()">
            @foreach($availableLanguages as $langId)
                <option value="{{ $langId }}" {{ $selectedLang == $langId ? 'selected' : '' }}>
                    {{ $langId }}
                </option>
            @endforeach
        </select>
    </form>

    <!-- 2. Create New SID Button -->
    <a href="{{ route('translations.create') }}" 
       style="background: #28a745; color: white; padding: 8px 15px; text-decoration: none; border-radius: 4px; font-weight: bold;">
       + Create New SID
    </a>
</div>

<!-- 3. Success Message (Optional but recommended) -->
@if(session('success'))
    <div style="color: green; padding: 10px; background: #e9f7ef; border: 1px solid #d4edda; margin-bottom: 15px;">
        {{ session('success') }}
    </div>
@endif

<!-- 4. Data Table -->
<table border="1" width="100%" style="border-collapse: collapse;">
    <thead>
        <tr style="background: #f8f9fa;">
            <th style="padding: 10px;">SID</th>
            <th style="padding: 10px;">Text ({{ $selectedLang }})</th>
        </tr>
    </thead>
    <tbody>
        @forelse($tableData as $row)
            <tr ondblclick="window.location='{{ route('edit', $row['sid']) }}?edit_lang={{ $selectedLang }}'" 
                style="cursor:pointer" 
                onmouseover="this.style.backgroundColor='#f1f1f1'" 
                onmouseout="this.style.backgroundColor='transparent'">
                <td style="padding: 10px;"><strong>{{ $row['sid'] }}</strong></td>
                <td style="padding: 10px;">{{ $row['text'] }}</td>
            </tr>
        @empty
            <tr>
                <td colspan="2" style="padding: 20px; text-align: center; color: #666;">
                    No translation resources found.
                </td>
            </tr>
        @endforelse
    </tbody>
</table>


