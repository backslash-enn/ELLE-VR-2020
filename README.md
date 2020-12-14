# ELLE VR 2020: Ellements of Learning
**Ellements of Learning** is a collection of fun, interactive mini games that focus on a different aspects of language learning. All mini games coexist in what we call "The Hubworld". The game is scalable because new mini games are able to be incorporated into the Hubworld.

## How to Incorporate a New Mini Game
After you make a new scene, follow these steps to incorporate your minigame into the existing ELLE system:

1. In the project directory, *Assets -> Prefabs -> General*, and drag in all the necessary prefabs.
    __VERY IMPORTANT: after dragging each prefab into the scene, right click -> prefab -> unpack, to ensure your changes do not change the template.__
  * Game Menu Prefab
    * Drag this into the scene, and the player can immediately browse modules and pick a module for the game
    * In the inspector you must specify 3 Music tracks: pre-game, in-game, and post-game music.
    * In the inspector you must also drag in a background. You may use a video file for a moving background, or an image for a static background.
    * If you use to use a video for the game menu background, make sure to check the "Use Video For Background* checkbox in the inspector.
  * Player Prefab
    * Drag this into the scene, and the player can now move with head and hand tracking, and players' glove colors will change based on the players' chosen skins.
  * Pause Menu
    * Drag this into the scene, and the pause menu *almsot* works out of the box. 
    * There are, however, a few parameters you must specify yourself. We will elaborate shortly.
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
        
            
    
* API Calls  
The following are the three API calls that must be included in a mini game.
_Detailed documentation on all the different API calls can be found at https://documenter.getpostman.com/view/11718453/Szzhdy4c?version=latest#intro_
  * startsession
    * Stores the starting time, ID of the module to be played, date of the session, and in which platform it is being played. Returns the sessionID that can be used when logging the user's answers and to end the session.
  * loggedanswer
  * endsession
    * End a previously started session. Provided the sessionID and the player's final score, this API updated the session record as ended and stores the score. Throws an error if trying to end a previously ended session and provided an non-exist sessionID.
  

 
