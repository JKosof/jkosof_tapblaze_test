<?php
$hostname = '127.0.0.1';
$username = 'root';
$password = '';
$database = 'roulettetest';
$secretKey = "mySecretKey";
 
try 
{
	$dbh = new PDO('mysql:host='. $hostname .';dbname='. $database, 
           $username, $password);
} 
catch(PDOException $e) 
{
	echo '<h1>An error has ocurred.</h1><pre>', $e->getMessage() 
            ,'</pre>';
}
	

	$sth = $dbh->prepare('INSERT INTO rouletteresults (`PlayerId`, `Sector`, `Reward`) VALUES (:id, :sector, :reward)');
	try 
	{
		$sth->bindParam(':id', $_POST['id'], 
                  PDO::PARAM_STR);
		$sth->bindParam(':sector', $_POST['sector'], 
                  PDO::PARAM_INT);
		$sth->bindParam(':reward', $_POST['reward'], 
                  PDO::PARAM_STR);
		$sth->execute();
	}
	catch(Exception $e) 
	{
		echo '<h1>An error has ocurred.</h1><pre>', 
                 $e->getMessage() ,'</pre>';
	}

?>
