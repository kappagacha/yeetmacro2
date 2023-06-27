![YeetMacro Engine](YeetMacro2/Resources/Images/appicon.svg) 
# YeetMacro
YeetMacro is a .NET MAUI android app that automates simple but grindy parts of mobile games.

![YeetMacro Engine](docs/yeetmacro_engine.drawio.svg)

Current target games are:
* Konosuba Fantastic Days
* Eversoul

This project is heavily influenced by [Fate Grand Automata (FGA)](https://github.com/Fate-Grand-Automata/FGA) and would not have been possible without it.

## ❗ Warning
Please do not use without knowing the risks. The app does randomize tap locations (see [initJavaScriptContext.js](https://github.com/kappagacha/yeetmacro2/blob/a3112d2af60784a2b3cdf58005dd1db0e2bb8223/YeetMacro2/Resources/Raw/initJavaScriptContext.js#L88)) but that does not guarantee that you will not get banned. If you have spent a lot of money on your account, it is recommended that you do not risk it.

## 📓 Setup
### 💻 Emulator
* Set resolution to 1920x1080 (or 1080x1920 depending on the game)
* Sideload apk file from [latest release](https://github.com/kappagacha/yeetmacro2/releases/tag/latest)

### 📱 Physical Android Device (recommended since games crash often on emulators)
* Set resolution to 1920x1080 (or 1080x1920 depending on the game) using [Resolution Changer - Uses ADB](https://play.google.com/store/apps/details?id=com.draco.resolutionchanger&hl=en_US&gl=US&pli=1)
    * Requires a computer with adb installed and enabling developer mode on the phone
    * Follow the direction of the app
        * [Set up adb on your computer](https://www.xda-developers.com/install-adb-windows-macos-linux/)
        * Connect device to computer
        * Open a command prompt and enter
            * `adb shell pm grant com.draco.resolutionchanger android.permission.WRITE_SECURE_SETTING`
* Sideload apk file from [latest release](https://github.com/kappagacha/yeetmacro2/releases/tag/latest)

### ⬇️ Download a MacroSet
* In YeetMacro app
* Go to Macro Manager Tab
* Tap on Globe icon and choose target game

### 🏃 Running
* Tap on Start
* Allow displaying over screen
* Enable Accessibility for YeetMacro Service
* Tap on play icon on the action overlay
* Choose the script
* Tap play icon on scripts overlay on the lower right

### 🔄 Updating MacroSet
* In YeetMacro app
* Go to Macro Manager Tab
* Tap on Update icon next to MacroSet name

### 🔎 Troubleshooting
* App randomly crashes
    * This is a known issue, I don't know how to fix it
* BlueStacks
    * Settings => Graphics => Graphics engine mode => Compatibility