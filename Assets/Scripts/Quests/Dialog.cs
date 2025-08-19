public class Dialog
{
    public string speakerName; // Nom de la personne qui parle
    public string message;     // Texte du dialogue

    public Dialog(string speakerName, string message)
    {
        this.speakerName = speakerName;
        this.message = message;
    }

    // Optionnel : une méthode pour afficher le dialogue dans la console
    public void Display()
    {
        UnityEngine.Debug.Log($"{speakerName} : {message}");
    }
}
