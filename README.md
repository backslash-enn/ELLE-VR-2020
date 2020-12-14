# ELLE VR 2020: Ellements of Learning
**Ellements of Learning** is a collection of fun, interactive mini games that focus on a different aspects of language learning. All mini games coexist in what we call "The Hubworld". The game is scalable because new mini games are able to be incorporated into the Hubworld.

## How to Incorporate a New Mini Game
After you make a new scene, follow these steps to incorporate your minigame into the existing ELLE system:

1. In the project directory, *Assets -> Prefabs -> General*, and drag in all the necessary prefabs.
    __IMPORTANT: after dragging each prefab into the scene, right click -> prefab -> unpack, to ensure your changes do not change the template.__
    * Game Menu Prefab
        * Drag this into the scene, and the player can immediately browse modules and pick a module for the game
        * In the inspector you must specify 3 Music tracks: pre-game, in-game, and post-game music.
        * In the inspector you must also drag in a background. You may use a video file for a moving background, or an image for a static background.
        * If you use to use a video for the game menu background, make sure to check the "Use Video For Background* checkbox in the inspector.
    * Player Prefab
        * Drag this into the scene, and the player can now move with head and hand tracking, and players' glove colors will change based on the players' chosen skins.
    * Pause Menu
        * Drag this into the scene, and the pause menu should work out of the box. 
    * Poof Prefab
        * *Optional:* You can use poof prefab to transition from the game menu to starting the game. See existing games for examples. 
2. There is some code you must write on your own, and an existing script you must drag in from *Assets -> Scripts -> Utility*, to tie everything together.
    * You should have a manager script that is a top-level manager for your game. This script must do a few things:
        * The most important thing is interacting with the game menu. First, you must get a reference to the GameMenu script in the scene
        * Next, you must write a method that will run to kick off the game. Then, you will tell the GameMenu what that method is by assigning it's onStartGame delegate
            * *Example*: void Start () { myGameMenuReference.onStartGame = MyStartGameMethod; }
         * From there, when the player picks a module from the game menu, the menu will auto disable itself, the pre-game music fades, and your method will be called.
         * Keep in mind that you want the game menu to be highly visible to the player pre-game, but at this point you will want to move the menu out of the way to start the game. How you implement these things is up to you (see the existing two games for two examples)
         * In your manager script, to actually do the game you will need a reference to myGameMenuReference.termList. This is where you can find the list of terms from the chosen module. If your game uses full questions instead of terms, you can access this using myGameMenuReference.questionList.
         * Finally, you will most likely have some intro sequence to start the game. When this sequence is over, and the actual game starts, use myGameMenuReference.StartInGameMusic() to...well I think you can guess. At this point you can do whatever you want for the game.
         * When you want to end the game, call myGameMenuReference.EndGame(int playerCorrectQuestionCount, int playerAttemptedQuestionCount). 
         * Finally, the last thing you will want to do, after the transition to the end game screen is complete is call myGameMenuReference.StartPostGameMusic().
         * A few more things that are not essential but your game may find useful:
            * myGameMenuReference.moduleList gives you the list of modules the player sees in the menu
            * myGameMenuReference.currentModule gives you the module the player picked from the menu
            * setting myGameMenuReference.goodToLeave = false disables using the b button in the main menu to leave, which may or may not be useful to your game
    * There is also a script in *Assets -> Scripts -> Utility* called VRInput. Put this script on a Gameobject in the scene. It has static fields for VR Input
        * has public static bools a, b, x, y, leftStickClick, rightStickClick, and startButton. The bool is true iff the corresponding button is pressed.
            * *Example*: VRInput.b. This is a static call.
        * has public static bools leftTriggerDigital, rightTriggerDigital, leftGripDigital, and rightGripDigital. The bool is true iff the trigger or grip is pressed more than a certain threshold. This threshold is a float called digitalThreshold, and you can change it on the scrip's inspector. Default value is 0.3.
        * There are also leftTrigger, rightTrigger, leftGrip, and rightGrip, which are analog floats from [0, 1].
        * You can access stick input with leftStick and rightStick, which are Vector2s (e.g. VRInput.rightStick.x).
        * All of the aforementioned bools have versions with "Down" or "Up" after the name (e.g. VRInput.yUp or VRInput.rightStickClickDown) that are only true on the frame the corresponding button is pressed or released.
        * You can make a controller vibrate with a set amount of time with LeftHandVibrationEvent(float strength, float duration) or RightHandVibrationEvent(float strength, float duration)
        * You can turn on/off a vibration that doesn't stop with LeftHandContinuousVibration(bool on, float strength) or RightHandContinuousVibration(bool on, float strength)
            * *Example*: VRInput.LeftHandContinuousVibration(true, 0.1f);
3. Now, to interact with the ELLE backend properly, you will have to make calls to the ELLEAPI class. Drag the ELLEAPI script from *Assets -> Scripts -> Utility* to a Gameobject in the scene.
    * We want the minigames to accommodate left handed players. At the start of your game, check ELLEAPI.rightHanded, and if it's false do what your game has to do to make it compatible with lefties.
        * Note that this call, like all the ELLEAPI methods, is a static call.
    * When your game first starts, call ELLEAPI.StartSession(int moduleID, bool isEndless). This method returns an int sessionID for the session. Hold onto this value.
        * You can get the moduleID from the game menu with myGameMenuReference.currentModule.moduleID
        * You acan get the game mode (which is an enum that can be either Quiz or Endless) with myGameMenuReference.currentGameMode, and only pass true if that is set to Endless
    * When your player answers a question in your game, call ELLEAPI.LogAnswer(int sessionID, Term term, bool gotAnswerCorrect, bool isEndless).
        * sessionID is the same session ID returned by the first call
        * Term is actually the Term object for the term attempted by the player. Recall that all Terms are availible via myGameMenuReference.termList
    * Finally, when a player finishes the game, call ELLEAPI.EndSession(int sessionID, int score)
        * __IMPORTANT: score is the number of questions the player got correct, NOT the percentage of questions the player got correct__
    * If you want more details, or want to make API calls directly, check out the ELLEAPI script. If you want more info what backend API calls are availible, visit https://documenter.getpostman.com/view/11718453/Szzhdy4c?version=latest#intro
4. Make your game compatible with Endless mode, which the player can pick in the game menu. This has three parts:
        * Since the Endless mode is, well, endless, you will want to modify your game so that it never ends if the player picks Endless mode. You can tell which mode the player picked using myGameMenuReference.currentGameMode.
        * In Endless mode the player can choose which terms from the module are valid in play. You can get which terms the players chose using the array of booleans myGameMenuReference.termEnabled. For each corresponding index in myGameMenuReference.termList[i], myGameMenuReference.termEnabled[i] is true iff the player checked this term in the Endless menu. Use this to leave out terms that were not picked from your game.
        * This was specified in the ELLEAPI section, but recall that some API calls have a flag that asks whether you are in Endless mode, so do not forget to update these.
            
            
## How to Place a New Mini Game in the Hubworld
Once you have your amazing new game, you will want to make it accssible to players via the Hubworld. This is a fairly straightforward process.

1. First things first, add your game scene to the build via *File -> Build Settings* and dragging your scene into the "Scenes In Build" window. Remember the scene name.
2. We call the little stations in the Hubworld used to access games __Kiosks__. To make a new one, go to *Assets -> Prefabs -> General* and drag in the Kiosk prefab.
    * __IMPORTANT: after dragging the prefab into the scene, right click -> prefab -> unpack, to ensure your changes do not change the kiosk template.____ 
3. In the scene object, put the visual display for the kiosk under *Kiosk -> Display Parent*
4. Attached to the Kiosk is the Kiosk script, which two public fields in the inspector you need to set.
    * Set the "Game Title" field to the name of the game. This name will pop up in-game when the player walks up to this Kiosk.
    * Set the "Game Scene Name" field to the name of the scene you dragged into the build settings ealier.

 
