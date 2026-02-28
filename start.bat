@echo off
chcp 65001 > nul
setlocal

echo =========================================
echo  くらべて上手くなる — 起動スクリプト
echo =========================================
echo.

:: ── .NET SDK の確認 ──────────────────────────
where dotnet > nul 2>&1
if errorlevel 1 (
    echo [エラー] .NET SDK が見つかりません。
    echo.
    echo 以下のページから .NET 8 SDK をダウンロードしてインストールしてください:
    echo   https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

for /f "tokens=*" %%v in ('dotnet --version 2^>nul') do set DOTNET_VER=%%v
echo [OK] .NET SDK バージョン: %DOTNET_VER%
echo.

:: ── ビルド ───────────────────────────────────
echo ビルド中... しばらくお待ちください。
echo.
dotnet build --nologo -v quiet
if errorlevel 1 (
    echo.
    echo [エラー] ビルドに失敗しました。
    echo 上のメッセージを確認し、問題を解決してから再度実行してください。
    echo.
    pause
    exit /b 1
)
echo.
echo [OK] ビルド成功。アプリを起動します...
echo.

:: ── 起動 ─────────────────────────────────────
dotnet run --project src\KurabeteNotebook.App --no-build

endlocal
