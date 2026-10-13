Add-Type -AssemblyName System.Windows.Forms

$ClientHash = "2469698B03B899B1A877A3C7DDC1554A5579C2F59DDF88062A1C177BE92EE04E"
$ServerHash = "924E48B5606ACBBE72F0A73931E2A805C9E3E2B59ADF275F0A3D02D706AC1B8B"

$Offset = 0x1856

function Backup-Exe {
    param([string]$Path)

    $BackupPath = "$Path.bak"

    if (Test-Path $BackupPath) {
        Write-Host "Backup already exists: $BackupPath" -ForegroundColor Yellow
        return
    }

    Copy-Item -Path $Path -Destination $BackupPath

    Write-Host "Backup created: $BackupPath" -ForegroundColor Green
}

function Select-Exe {
    $dialog = New-Object System.Windows.Forms.OpenFileDialog
    $dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"
    $dialog.Title = "Select TSOClient.exe"

    if ($dialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        return $dialog.FileName
    }

    return $null
}

function Get-ExeHash {
    param([string]$Path)

    return (Get-FileHash $Path -Algorithm SHA256).Hash.ToUpper()
}

function Test-HouseSimExe {
    param([string]$Path)

    $hash = Get-ExeHash $Path

    switch ($hash) {
        $ClientHash {
            return "Client"
        }

        $ServerHash {
            return "HouseSimServer"
        }

        default {
            Write-Host ""
            Write-Host "ERROR: This is not a recognized TSOClient executable." -ForegroundColor Red
            Write-Host "SHA256: $hash"
            return $null
        }
    }
}

function Get-ClientType {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $value = $bytes[$Offset]

    switch ($value) {
        0x01 {
            Write-Host "Type: HouseSimServer" -ForegroundColor Green
        }

        0x02 {
            Write-Host "Type: Client" -ForegroundColor Cyan
        }

        default {
            Write-Host ("Unexpected value at 0x{0:X}: 0x{1:X2}" -f $Offset, $value) -ForegroundColor Yellow
        }
    }
}

function Patch-ClientType {
param(
[string]$Path,
[byte]$NewValue,
[string]$TargetType
)
 
$currentType = Test-HouseSimExe $Path
 
if (-not $currentType) {
return
}
 
if ($currentType -eq $TargetType) {
Write-Host ""
Write-Host "This TSOClient is already a $TargetType." -ForegroundColor Yellow
return
}
 
Backup-Exe $Path
 
$bytes = [System.IO.File]::ReadAllBytes($Path)
 
$oldValue = $bytes[$Offset]
$bytes[$Offset] = $NewValue
 
[System.IO.File]::WriteAllBytes($Path, $bytes)
 
Write-Host ""
Write-Host "Patched successfully!" -ForegroundColor Green
Write-Host ("Offset 0x{0:X}: 0x{1:X2} -> 0x{2:X2}" -f $Offset, $oldValue, $NewValue)
 
$newHash = Get-ExeHash $Path
 
Write-Host ""
Write-Host "New SHA256:"
Write-Host $newHash
}

do {
    Clear-Host

    Write-Host "========================================"
    Write-Host "     nio2so - TSOClient.exe Patcher     "
    Write-Host "    For use with TSO: Pre-Alpha ONLY    "
    Write-Host "========================================"
    Write-Host ""
    Write-Host "1. Patch TSOClient to HouseSimServer"
    Write-Host "2. Patch TSOClient to Client"
    Write-Host "3. Check TSOClient Type"
    Write-Host "4. Exit"
    Write-Host ""

    $choice = Read-Host "Select an option"

    switch ($choice) {

        "1" {
            $exe = Select-Exe

            if ($exe) {
                Patch-ClientType -Path $exe -NewValue 0x01 -TargetType "HouseSimServer"
            }
        }

        "2" {
            $exe = Select-Exe

            if ($exe) {
                Patch-ClientType -Path $exe -NewValue 0x02 -TargetType "Client"
            }
        }

        "3" {
            $exe = Select-Exe

            if ($exe) {
                $type = Test-HouseSimExe $exe

                if ($type) {
                    Write-Host ""
                    Get-ClientType $exe
                }
            }
        }

        "4" {
            break
        }

        default {
            Write-Host "Invalid selection." -ForegroundColor Red
        }
    }

    if ($choice -ne "4") {
        Write-Host ""
        Pause
    }
	else {
			break;
	}

} while ($true)