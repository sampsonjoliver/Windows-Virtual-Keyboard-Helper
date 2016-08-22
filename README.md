#Windows Virtual Keyboard Helper

Designed for use on Windows 8 and above systems that operate a non-native touch-screen interface.

For example, this was used for a project using a Windows-based Kiosk machine with an infrared array that simulated clicks from user touch gestures, but the drivers simulated a mouse click rather than a touch click. This causes issues in Windows, as mouse clicks on text fields does not cause the OS to launch the virtual keyboard, but touch clicks do.

Hence, these utils provide a rough guide on several mechanisms that can be combined in various fashions to overcome this challenge.

## Touch Injector
This class is based on sample code provided by microsoft for testing, which programmatically injects touch events and particular screen coordinates.

## VirtualKeyboard
Marshals the user32.dll in order to resolve the PostMessage and FindWindow extern methods, and provides Launch and Close methods to hence send system messages to open the virtual keyboard process if its window does not already exist.

## Mouse Hook Listener
A Hook that listens for mouse click events, intercepts them, and hence allows the developer to trigger another event. For example, we can use the mouse hook to listen for mouse click events, intercept them, and replace them with injected Touch events.

Or, with the hook, we can trace the screen coordinates to determine if the object being clicked is a text input field, and use the ViertualKeyboard class to launch or find the virtual keyboard.
