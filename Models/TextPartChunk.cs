using System;
using Newtonsoft.Json;

namespace DocCreator01.Models
{
    public class TextPartChunk
    {
        public string? Text { get; set; }
        public Guid Id { get; set; }
        
        /// <summary>
        /// Оригинальное изображение в формате byte array
        /// </summary>
        [JsonProperty]
        public byte[]? ImageData { get; set; }
        
        /// <summary>
        /// Миниатюра изображения для отображения в UI
        /// </summary>
        [JsonProperty]
        public byte[]? ThumbnailData { get; set; }
        
        /// <summary>
        /// Проверяет, содержит ли чанк изображение
        /// </summary>
        [JsonIgnore]
        public bool HasImage => ImageData != null && ImageData.Length > 0;
    }
}
