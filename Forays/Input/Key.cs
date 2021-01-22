namespace Forays.Input
{
    public class Key
    {
        
        // Properties
        
        private KeyCode keyCode = KeyCode.None;

        // Getters
        
        public KeyCode GetKeyCode()
        {
            return keyCode;
        }

        // Setters
        
        public void SetKeyCode(KeyCode code)
        {
            keyCode = code;
        }
    }
}