
<?php
$hostname = '127.0.0.1';
$username = 'root';
$password = '';
$database = 'roulettetest';
$port = 3060;
 
try 
{
	$dbh = new PDO('mysql:host='. $hostname .';dbname='. $database, 
         $username, $password);
} 
catch(PDOException $e) 
{
	echo '<h1>An error has occurred.</h1><pre>', $e->getMessage()
            ,'</pre>';
}
 
$sth = $dbh->query('SELECT `Reward`, `Chance` FROM `roulettesectors` ORDER BY `Sector`');
$sth->setFetchMode(PDO::FETCH_ASSOC);
 
$result = $sth->fetchAll();
 
if (count($result) > 0) 
{
	foreach($result as $r) 
	{
		echo $r['Chance'], "\n _";
		echo $r['Reward'], "\n _";
	}
}
?>