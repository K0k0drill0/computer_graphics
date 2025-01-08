using System.Windows.Forms;

namespace Engine.SystemInteraction
{
    internal class NativeFileDialog
    {
        public static string OpenFileDialog(string path = "C:\\Users\\asana\\source\\repos\\Engine\\Engine\\Textures", 
                                            string filter = "Image Files (*.png)|*.png")
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = path;

                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    return openFileDialog.FileName;
            }
            
            return null;
        }
    }
}
