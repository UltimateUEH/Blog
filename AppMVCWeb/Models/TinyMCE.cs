namespace App.Models
{
    public class TinyMCE
    {
        public TinyMCE(string idEditor, bool loadLibrary = true)
        {
            IdEditor = idEditor;
            LoadLibrary = loadLibrary;
        }

        public string IdEditor { get; set; }

        public bool LoadLibrary { get; set; }

        public int Height { get; set; } = 300;

        public string Plugins { get; set; } = @"
            anchor autolink charmap codesample emoticons image imagetools link lists media searchreplace table visualblocks wordcount linkchecker elfinder
        ";

        public string Toolbars { get; set; } = @"
            undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link image media table | align lineheight | numlist bullist indent outdent | emoticons charmap | removeformat | elfinder
        ";
    }
}
