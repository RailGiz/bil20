<?php
// Установка параметров подключения к базе данных
$host = 'localhost';  // Адрес сервера базы данных
$db = 'bil20';  // Имя базы данных
$user = 'root';  // Пользователь базы данных
$password = '';  // Пароль базы данных

// Создание подключения к базе данных
$connection = new mysqli($host, $user, $password, $db);
if ($connection->connect_error) {
    die("Ошибка подключения к базе данных: " . $connection->connect_error);
}

// Запрос для получения изображений из базы данных
$sql = "SELECT id, image FROM Images";
$result = $connection->query($sql);
?>

<!DOCTYPE html>
<html>
<head>
    <title>Галерея изображений</title>
</head>
<body>
    <h1>Галерея изображений</h1>

    <?php
    // Проверка наличия изображений
    if ($result->num_rows > 0) {
        // Вывод изображений
        while ($row = $result->fetch_assoc()) {
            $imageId = $row['id'];
            $imageData = $row['image'];

            // Генерация временного имени файла
            $tempFileName = tempnam(sys_get_temp_dir(), 'image');
            // Сохранение изображения во временный файл
            file_put_contents($tempFileName, $imageData);

            // Вывод изображения на страницу
            echo "<img src='data:image/jpeg;base64," . base64_encode(file_get_contents($tempFileName)) . "' alt='Image $imageId'><br>";

            // Удаление временного файла
            unlink($tempFileName);
        }
    } else {
        echo "Изображения не найдены.";
    }

    // Закрытие соединения с базой данных
    $connection->close();
    ?>

</body>
</html>
