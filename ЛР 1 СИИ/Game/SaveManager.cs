using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace LR1_SAI
{
    public class SaveManager(string savePath)
    {
        private readonly string savePath = savePath;
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public DataType? ReadSave<DataType>()
        {
            try
            {
                CheckSavePath();
                using var readStream = File.OpenRead(savePath);
                return JsonSerializer.Deserialize<DataType>(readStream, options);
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при сохранении: {ex.Message}");
                throw;
            }
        }

        public void Save<DataType>(DataType data)
        {
            try
            {
                CheckSavePath();
                using var writeStream = File.Create(savePath);
                JsonSerializer.Serialize(writeStream, data, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при сохранении: {ex.Message}");
                throw;
            }
        }

        private void CheckSavePath()
        {
            if (string.IsNullOrWhiteSpace(savePath))
                throw new ArgumentException("Путь к файлу не установлен");

            if (!File.Exists(savePath))
                throw new FileNotFoundException($"Файл {savePath} не найден");
        }
    }
}
