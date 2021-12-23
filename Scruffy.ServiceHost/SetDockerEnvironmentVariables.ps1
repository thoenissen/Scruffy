foreach($line in Get-Content .\Scruffy.ServiceHost\Docker.env) 
{
	$tmp = $line.Split("=")

	[System.Environment]::SetEnvironmentVariable($tmp[0], $tmp[1])
}