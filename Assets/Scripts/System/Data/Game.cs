﻿using UnityEngine;
using System.IO;
using System.Collections;
using SimpleJSON;

[System.Serializable]
public class Game
{
    public DirectoryInfo directory;

    public string name;
    public string author;
    public Sprite screenshot;
    public string executable;

    public enum GameType { EXE, PICO8, CUSTOM, LEGACY, FLASH }
    public GameType gameType;

    public bool voidGame = false;

    private JSONNode savedMetadata;

    /// <summary>
    /// The game only takes in a directory handed down by the Playlist class, it then finds all the relevant information for the Game
    /// </summary>
    /// <param name="directory">The directory path.</param>
    public Game(string directory) {
        this.directory = new DirectoryInfo(directory);

        //Check for the Winnitron Metadata JSON, and use oldschool folder naming if it doesn't exist
        string metadata = Path.Combine(this.directory.FullName, "winnitron_metadata.json");
        if (System.IO.File.Exists(metadata)) {
            BuildGameJSON();
        } else {
            BuildGame();
        }

        BuildHelperScripts();
    }

    /// <summary>
    /// Builds a game from scratch using only what it can find in the directory.
    /// </summary>
    private void BuildGame() {
        GM.logger.Warn("GAME: No JSON!  Determining game...");

        if (DetermineGameType()) {
            this.name = GetGameNameFromFolderName();
            this.screenshot = GetScreenshot();
        } else {
            GM.logger.Error("GAME: Could not determine game type.  Voiding game " + directory);
            voidGame = true;
        }
    }

    /// <summary>
    /// Builds a game using metadata information found in a JSON file generated by Winnitron Network or manually.
    /// </summary>
    private void BuildGameJSON() {
        savedMetadata = GM.data.LoadJson(Path.Combine(directory.FullName, "winnitron_metadata.json"));

        this.name = savedMetadata["title"];
        this.author = null; //No author stuff just yet
        this.screenshot = GetScreenshot();
        this.executable = Path.Combine(directory.FullName, savedMetadata["executable"]);

        switch(savedMetadata["keys"]["template"]) {
            case "default":
                gameType = GameType.EXE;
                break;

            case "pico8":
                gameType = GameType.PICO8;
                break;

            case "flash":
                gameType = GameType.FLASH;
                break;

            case "legacy":
                gameType = GameType.LEGACY;
                break;

            case "custom":
                gameType = GameType.CUSTOM;
                break;

            default:
                DetermineGameType();
                break;
        }

        GM.logger.Info(null, "Game Built JSON! Name: " + name + " Screenshot: " + screenshot.name + " exe path: " + executable);
    }






    private Sprite GetScreenshot() {
        // Load the screenshot from the games directory as a Texture2D
        var screenshotTex = new Texture2D(1024, 768);

        if (Directory.GetFiles(this.directory.ToString(), "*.png").Length > 0)
        {
            GM.logger.Info("GAME: Loading custom screenshot " + Directory.GetFiles(this.directory.ToString(), "*.png")[0]);
            screenshotTex.LoadImage(File.ReadAllBytes(Directory.GetFiles(directory.FullName + Path.DirectorySeparatorChar, "*.png")[0]));
        }
        else if (gameType == GameType.PICO8)
        {
            GM.logger.Info("GAME: Loading default PICO8 screenshot ");
            screenshotTex = Resources.Load<Texture2D>("default_images/pico8") as Texture2D;
        }
        else
        {
            GM.logger.Info("GAME: Loading default screenshot");
            screenshotTex = Resources.Load<Texture2D>("default_images/exe") as Texture2D;
        }

        // Turn the Texture2D into a sprite
        return Sprite.Create(screenshotTex, new Rect(0, 0, screenshotTex.width, screenshotTex.height), new Vector2(0.5f, 0.5f));
    }

    private string GetExecutablePath() {
        //Find the .exe in the directory and save a reference
        return Path.Combine(directory.FullName, executable);
    }

    private string GetGameNameFromFolderName() {
        //Figure out the name of the game from the directory title
        var directoryName = directory.Name;

        //Replace the underscores and dashes with blank spaces
        var name = directoryName.Replace('_', ' ');
        name = name.Replace('-', ' ');

        return name;
    }

    /// <summary>
    /// Figures out what kind of game might be in the directory depending on the files inside.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful in determining game type.</returns>
    private bool DetermineGameType() {
        if (Directory.GetFiles(this.directory.ToString(), "*.html").Length == 1)
        {
            executable = Directory.GetFiles(this.directory.ToString(), "*.html")[0];
            GM.logger.Info("Determined PICO8! " + executable);
            gameType = GameType.PICO8;
            return true;
        }
        else if (Directory.GetFiles(this.directory.ToString(), "*.exe").Length == 1)
        {
            GM.logger.Info("Determined EXE!");
            executable = Directory.GetFiles(this.directory.ToString(), "*.exe")[0];
            gameType = GameType.EXE;
            return true;
        }

        //Can't determine game type, voiding this game
        voidGame = true;
        return false;
    }

    /// <summary>
    /// This will make the launcher AHK scripts, and/or other scripts (.html files in pico8 case)
    /// and put them in the same folder as the game.
    /// </summary>
    public void BuildHelperScripts() {
        GM.logger.Info("GAME: Create scripts for game " + name);

        string newAHKfile = "";
        string execFile = Path.GetFileName(executable);

        switch (gameType) {
            case Game.GameType.EXE:

                newAHKfile = Resources.Load<TextAsset>("AHK_templates/ExeGameTemplate").text;

                newAHKfile = newAHKfile.Replace("{GAME_PATH}", executable);
                newAHKfile = newAHKfile.Replace("{GAME_NAME}", name);

                break;

            case Game.GameType.PICO8:

                string newJS = Resources.Load<TextAsset>("Pico8Launcher").text;
                newJS = newJS.Replace("{{{PATH_TO_HTML}}}", executable.Replace("\\", "\\\\"));
                WriteStringToFile(newJS, "Pico8Launcher.js");

                newAHKfile = Resources.Load<TextAsset>("AHK_templates/Pico8GameTemplate").text;
                newAHKfile = newAHKfile.Replace("{GAME_PATH}", GM.options.dataPath + "/Options/Pico8/nw.exe");

                break;

            case Game.GameType.LEGACY:

                newAHKfile = Resources.Load<TextAsset>("AHK_templates/ExeGameTemplate").text;

                newAHKfile = newAHKfile.Replace("{GAME_PATH}", executable);
                newAHKfile = newAHKfile.Replace("{GAME_NAME}", name);

                break;

            case Game.GameType.FLASH:

                newAHKfile = Resources.Load<TextAsset>("AHK_templates/FlashGameTemplate").text;

                newAHKfile = newAHKfile.Replace("{GAME_PATH}", executable);
                newAHKfile = newAHKfile.Replace("{GAME_NAME}", name);

                break;

            case Game.GameType.CUSTOM:
                break;
        }

        //Things needed for every Launcher Script

        //Replace variables
        newAHKfile = newAHKfile.Replace("{GAME_FILE}", execFile);
        newAHKfile = newAHKfile.Replace("{DEBUG_OUTPUT}", "true"); // TODO make this configurable
        newAHKfile = newAHKfile.Replace("{IDLE_TIME}", "" + GM.options.runnerSecondsIdle);
        newAHKfile = newAHKfile.Replace("{IDLE_INITIAL}", "" + GM.options.runnerSecondsIdleInitial);
        newAHKfile = newAHKfile.Replace("{ESC_HOLD}", "" + GM.options.runnerSecondsESCHeld);

        newAHKfile = insertKeyMapping(newAHKfile);

        //Delete old file and write to new one
        WriteStringToFile(newAHKfile, "RunGame.ahk");
    }

    /// <summary>
    /// Called by RUNNER script, in case the game needs to do some extra setup before it runs.
    /// </summary>
    public void PreRun() {
        if (gameType == GameType.PICO8)
        {
            string source = Path.Combine(directory.ToString(), "Pico8Launcher.js");
            string dest   = Path.Combine(GM.options.dataPath, "Options/Pico8/Pico8Launcher.js");

            GM.logger.Info("GAME: PreRun copying " + source + " to " + dest);
            File.Copy(source, dest, true);
        }
    }


    private string insertKeyMapping(string ahkFile) {
        string keymap = "";
        JSONNode parsedBindings = getKeyBindings();
        ArrayList gameKeys = allGameKeys(parsedBindings);

        for(int pNum = 1; pNum <= 4; pNum++) {
            JSONNode playerKeys;

            try {
                playerKeys = parsedBindings[pNum.ToString()];
            } catch (System.NullReferenceException) {
                break;
            }

            foreach(string control in KeyBindings.CONTROLS) {
                KeyCode key = GM.options.keys.GetKey(pNum, control);
                string launcherKey = GM.options.keyTranslator.toAHK(key);
                string gameKey = playerKeys[control];

                if (pNum > savedMetadata["max_players"].AsInt || gameKey == null) {
                    gameKey = "return";
                }

                bool keyAlreadyMapped = gameKeys.Contains(launcherKey);
                if (!keyAlreadyMapped && (launcherKey != gameKey)) {
                    keymap += (launcherKey + "::" + gameKey + "\n");
                }
            }
        }

        return ahkFile.Replace("{KEYMAP}", keymap);
    }

    private ArrayList allGameKeys(JSONNode bindings) {
        ArrayList keys = new ArrayList();

        for(int pNum = 1; pNum <= 4; pNum++) {
            foreach(string control in KeyBindings.CONTROLS) {
                string key = bindings[pNum.ToString()][control];

                if (key != null)
                    keys.Add(key);
            }
        }

        return keys;
    }

    private JSONNode getKeyBindings() {
        string tmpl = savedMetadata["keys"]["template"];
        JSONNode bindings = null;

        string tmplFile = Path.Combine(GM.options.defaultOptionsPath, "keymap_templates.json");
        JSONNode bindingTemplates = GM.data.LoadJson(tmplFile);

        if (tmpl == null) {
            if (savedMetadata["keys"]["bindings"] == null) {
                GM.logger.Debug("No key binding info provided for " + name + ". Using defaults.");
                tmpl = "default";
            } else {
                tmpl = "custom";
            }
        }

        switch(tmpl) {
            case "custom":
                GM.logger.Debug("Loading custom key bindings for " + name);
                bindings = savedMetadata["keys"]["bindings"];
                break;

            case "default":
            case "legacy":
            case "flash":
            case "pico8":
                GM.logger.Debug("Loading " + tmpl + " bindings from tmplFile: " + tmplFile);
                bindings = bindingTemplates[tmpl];
                break;

            default:
                GM.logger.Error("Invalid key template type '" + tmpl + "' for " + name + ". (Using default.) Valid templates are 'default', 'legacy', 'pico8', 'custom'.");
                GM.logger.Debug("Loading " + tmpl + " bindings from tmplFile: " + tmplFile);
                bindings = bindingTemplates["default"];
                break;
        }

        // Remove controls for players that don't exist.
        for (int p = savedMetadata["max_players"].AsInt + 1; p <= 4; p++) {
            bindings.Remove(p.ToString());
        }

        return bindings;
    }

    /// <summary>
    /// Writes the text to the filename in the Game directory.
    /// </summary>
    /// <param name="text">The text to encode into the file.</param>
    /// <param name="fileName">The name of the file.</param>
    private void WriteStringToFile(string text, string fileName) {
        string file = Path.Combine(directory.FullName, fileName);

        File.Delete(file);
        System.IO.File.WriteAllText(file, text);
        GM.logger.Info("GAME: Writing file " + file);
    }
}
