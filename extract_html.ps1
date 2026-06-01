$c = Get-Content 'C:\Users\nguye\Downloads\D _ Glass effect blue _ library imagery  _NEW_ (1).html' -Raw

# Find all text content between > and <
$matches = [regex]::Matches($c, '>([^<]+)<')
$textContent = @()
foreach ($m in $matches) {
    $text = $m.Groups[1].Value.Trim()
    if ($text.Length -gt 1 -and $text -notmatch '^[\s\r\n]+$' -and $text -notmatch '^@font-face' -and $text -notmatch '^d09GM' -and $text.Length -lt 200) {
        $textContent += $text
    }
}
Write-Output "=== TEXT CONTENT IN HTML ==="
$textContent | Select-Object -Unique | ForEach-Object { Write-Output $_ }

Write-Output ""
Write-Output "=== LOOKING FOR DESIGN STRUCTURE ==="

# Find class names
$classMatches = [regex]::Matches($c, 'class="([^"]+)"')
$classes = @()
foreach ($m in $classMatches) {
    $classes += $m.Groups[1].Value
}
Write-Output "Classes found:"
$classes | Select-Object -Unique | ForEach-Object { Write-Output "  - $_" }

Write-Output ""
Write-Output "=== LOOKING FOR SVG/IMG/INPUT elements ==="
$svgCount = ([regex]::Matches($c, '<svg')).Count
$imgCount = ([regex]::Matches($c, '<img')).Count  
$inputCount = ([regex]::Matches($c, '<input')).Count
$buttonCount = ([regex]::Matches($c, '<button')).Count
Write-Output "SVG elements: $svgCount"
Write-Output "IMG elements: $imgCount"
Write-Output "INPUT elements: $inputCount"
Write-Output "BUTTON elements: $buttonCount"

Write-Output ""
Write-Output "=== LOOKING FOR CSS VARIABLES ==="
$varMatches = [regex]::Matches($c, '--([\w-]+):\s*([^;]+)')
$vars = @{}
foreach ($m in $varMatches) {
    $varName = $m.Groups[1].Value
    $varValue = $m.Groups[2].Value
    if (-not $vars.ContainsKey($varName)) {
        $vars[$varName] = $varValue
    }
}
foreach ($key in ($vars.Keys | Sort-Object)) {
    Write-Output ("  --" + $key + ": " + $vars[$key])
}
