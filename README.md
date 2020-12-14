# ELLE VR 2020: Ellements of Learning
**Ellements of Learning** is a collection of fun, interactive mini games that focus on a different aspects of language learning. All mini games coexist in what we call "The Hubworld". The game is scalable because new mini games are able to be incorporated into the Hubworld.

## How to Incorporate a New Mini Game
First, create a new scene. In the project directory, *Assets -> Prefabs -> General*, you can find all the necessary tools to make a new scene.
    
* Prefabs  
__VERY IMPORTANT: right click -> prefab -> unpack, to ensure your changes do not change the template.__
  * Game Menu Prefab
    * Module select menu works out of the box.
  * Player Prefab
    * Get VR hands with tracking out of the box.
  * Pause Menu
    * Pause/unpause functionality is mostly drag and drop (there are a few things you have to put in code to make it work).
  * Poof Prefab
    * *Optional:* Use poof prefab to transition from menu to game.
    
* API Calls  
The following are the three API calls that must be included in a mini game.
_Detailed documentation on all the different API calls can be found at https://documenter.getpostman.com/view/11718453/Szzhdy4c?version=latest#intro_
  * startsession
    * Stores the starting time, ID of the module to be played, date of the session, and in which platform it is being played. Returns the sessionID that can be used when logging the user's answers and to end the session.
  * loggedanswer
  * endsession
    * End a previously started session. Provided the sessionID and the player's final score, this API updated the session record as ended and stores the score. Throws an error if trying to end a previously ended session and provided an non-exist sessionID.
  

 
