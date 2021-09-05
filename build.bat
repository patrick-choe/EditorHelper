dotnet "C:\Program Files\dotnet\sdk\5.0.400\MSBuild.dll" /p:Configuration=Release
mkdir Release
cd ./Release
mkdir EditorHelper
cp ../EditorHelper/bin/Release/EditorHelper.dll ./EditorHelper/EditorHelper.dll
cp ../EditorHelper/bin/Release/MathParser.org-mXparser.dll ./EditorHelper/MathParser.org-mXparser.dll
cp ../Info.json ./EditorHelper/Info.json
tar -acf EditorHelper-Release.zip EditorHelper
mv EditorHelper-Release.zip ../
cd ../
rm -rf Release
pause
