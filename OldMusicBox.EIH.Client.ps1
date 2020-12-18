# /bin/release
$dll462  = Resolve-Path "./OldMusicBox.EIH.Client/bin/Release/OldMusicBox.EIH.Client.dll"
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll462).FileVersion

# utwórz folder
$path = ("./OldMusicBox.EIH.Client/bin/" + $version)

New-Item -ItemType Directory -Force -Path $path
New-Item -ItemType Directory -Force -Path ($path + "/lib")
New-Item -ItemType Directory -Force -Path ($path + "/lib/net462")

Copy-Item $dll462 ($path + "/lib/net462")

# nuspec
$nuspec = ($path + "/OldMusicBox.EIH.Client.nuspec")
$contents = @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>OldMusicBox.EIH.Client</id>
    <version>__VERSION</version>
    <authors>wzychla</authors>
	<dependencies>
		<group targetFramework="net462">
			<dependency id="BouncyCastle" version="1.8.6.1" />
		</group>
	</dependencies>
    <projectUrl>https://github.com/wzychla/OldMusicBox.EIH.Client</projectUrl>
	<repository type="git" url="https://github.com/wzychla/OldMusicBox.EIH.Client.git" />
    <requireLicenseAcceptance>true</requireLicenseAcceptance>
	<license type="expression">MIT</license>
    <description>OldMusicBox.EIH.Client. Independent Węzeł Krajowy implementation.</description>
    <copyright>Copyright 2020 Wiktor Zychla</copyright>
	<icon>images\icon.png</icon>
    <tags>SAML2, Węzeł Krajowy</tags>
  </metadata>
  <files>
	<file src="lib/net462/OldMusicBOx.EIH.Client.dll" target="lib/net462/OldMusicBox.EIH.Client.dll" />  
	<file src="..\..\..\icon.png" target="images\" />
  </files>
</package>
"@

$contents = $contents -replace "__VERSION", $version

#New-Item -ItemType File -Path $nuspec 
Set-Content -Path $nuspec -value $contents -Encoding "UTF8"

# spakuj
Set-Location -Path $path
nuget pack

Set-Location -Path "../../.."