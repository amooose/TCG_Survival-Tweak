# TCG_Survival-Tweak
The Coin Game Survival Tweak  
A mod to disable the Curfew, change the Health/Energy degradation speed, and time passing rate.

## Install
* [BepInEx Install Required](https://github.com/BepInEx/BepInEx/releases). Install to the root directory of TheCoinGame.
* Download the latest release of TCG_Survival-Tweak.zip and place the BepInEx folder in the root directory of TheCoinGame  
(There should already be a BepinEx folder there since you installed it), merge the folders.

## Configure
* Open the config file at `TheCoinGame\BepInEx\config`
### Health Degradation
* The rate at which your health decreases, 0.06 is the default (Lower = slower)

### Energy Degradation Max
* The max rate at which your health decreases, 0.06 is the default max. (Lower = slower)  
* Energy degrades from 0.005 to 0.06 depending on what you consume.
You may set the cap to any value, even below 0.005.

### Time Passing Rate
* The rate at which time passes, 0.05 is the default (Lower = faster)

### No Curfew
* Disables the curfew, return home whenever you want. (Time caps at midnight due to game code)

## TheCoinGame Modding Tips:
* You wont find much help within Assembly-CSharp.dll, as this game uses PlayMaker, which utilizes Finite State Machines for scripting.
* [HollowKnightFSMView](https://github.com/nesrak1/HollowKnightFSMView) is useful for visualing the FSMs. I've included the needed cldb.dat for the 2018.2.21 version of unity in the cldb folder of this repo.
* Compile with Net Framework 4.

