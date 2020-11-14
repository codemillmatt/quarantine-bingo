# Quarantine Bingo

![bingo game drawing](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1588012752/93625_vveito.jpg)

As I write this it's April of 2020, so you know what that means... I'm stuck inside of my house. And my extended family is stuck inside of their houses too.

So to help pass the time and keep those family bonds strong ... we decided to have a weekly family game night over FaceTime and play bingo!

So I decided to write a quick app to help auto recognize bingo numbers as they're called - then tell me if I won or not.

## Quarantine Bingo!

So here's what I built. It's a simple Xamarin.Forms app that displays a bingo card.

I _could_ tap on the numbers as they're being called... but it's much more fun to let the iPad do it for me!

And it tells me if I've won the game or not.

![game in action](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/e_shadow:40/v1588009518/ScreenFlow_glsnrg.gif)

What's not to like?

## Cognitive Services Speech to Text

To figure out what number has been called I'm using one of the lesser known [Cognitive Services - that of Speech to Text](https://docs.microsoft.com/azure/cognitive-services/speech-service/quickstarts/speech-to-text-from-microphone?WT.mc_id=mobile-0000-masoucou).

What Speech to Text does is pretty simple. It uses the microphone to listen to a stream of words, then translates those words into a string, then returns it to you. 

(It also does a whole lot more - the SDK has functionality to ["listen" for wake words](https://docs.microsoft.com/azure/cognitive-services/speech-service/how-to-choose-recognition-mode?WT.mc_id=mobile-0000-masoucou) - think along the lines of Cortana!)

And there's a free tier so I won't have to pay anything!

Sweet!

So all I would need to do is to write a little bit of code to "listen" for numbers, check if the card is displaying that number, then voila - bingo time!

## Let's play bingo!

There's three parts to getting Speech to Text working.

* Setup the Speech API Cognitive Service in the Azure portal
* Install the NuGet package
* Make some changes to the `Info.plist` and `AndroidManifest.xml` files.
* Write the code!

### Setup Azure

Go to the Azure portal - then [startup the CLI](https://docs.microsoft.com/azure/cloud-shell/quickstart?view=azure-cli-latest&WT.mc_id=mobile-0000-masoucou). Then enter these commands:

```language-bash
RESOURCE_GROUP_NAME = "PUT THE NAME OF YOUR RESOURCE GROUP HERE CALL IT WHATEVS"
SPEECH_SERVICE_NAME = "PUT THE NAME OF YOUR SPEECH SERVICE HERE THE NAME IS YOUR CHOICE"

az cognitiveservices account create -n $SPEECH_SERVICE_NAME -g $RESOURCE_GROUP_NAME --kind SpeechServices --sku F0 -l westus
az cognitiveservices account keys list -n $SPEECH_SERVICE_NAME -g $RESOURCE_GROUP_NAME
```

### Install the NuGet package

Back into whatever Xamarin project you want to run the Speech to Text from.

Install the following NuGet: [Microsoft.CognitiveServices.Speech](https://www.nuget.org/packages/Microsoft.CognitiveServices.Speech/)

If you're doing Forms - you're gonna have to put that in the platform and the Forms projects.

### Make the platform changes

There's a couple of things that you need to change on each platform project.

First you need to tell iOS what to display when it prompts the user for access to the microphone. In to the `Info.plist`

Add the following key to it:

```language-xml
<key>NSMicrophoneUsageDescription</key>
<string>Transcribe Bingo Cards</string>
```

Over on the Droid side of things - pop open the `AndroidManifest.xml` file and add:

```language-xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
```

Done!

### Listen and transcribe!

Now you'd think this would be the difficult part. You have to prompt for the microphone permissions. Then somehow get the audio stream from the mic. Buffer all that up and send it to Azure. Rely on some kind of weird callback to get the transcribed text back... ugh.

Turns out it's not too bad!

Prompting for the permissions is a breeze thanks to [`Xamarin.Essentials`](https://docs.microsoft.com/xamarin/essentials/permissions?WT.mc_id=mobile-0000-masoucou). It'll look something like this:

```language-csharp
public async Task<PermissionStatus> CheckAndRequestMicrophonePermission()
{
    var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

    if (status != PermissionStatus.Granted)
    {
        status = await Permissions.RequestAsync<Permissions.Microphone>();
    }

    return status;
}
```

Call it before turning on the recording. If the `PermissionStatus` is anything but `Granted` then prompt the user to give the app microphone capabilities.

Now the listen and transcribe part. Here's the function I'm using to start up a continuous listen and transcribe:

```language-csharp
using Microsoft.CognitiveServices.Speech; // this is the namespace of the speech to text stuff

SpeechRecognizer recognizer; // this is a class level variable

public async Task StartTranscription()
{
    if (recognizer == null)
    {
        var config = SpeechConfig.FromSubscription("<PUT YOUR KEY HERE>", "westus");

        recognizer = new SpeechRecognizer(config);

        recognizer.Recognized += Recognizer_Recognized;
    }

    if (isRecognizing)
        return;

    await recognizer.StartContinuousRecognitionAsync();


    isRecognizing = true;
}
```

Notice that `recognizer.Recognized` event. That's what's gonna get called every time some text comes back.

But notice what's _not_ there... nothing about handling the mic... or handling the bytes and bytes of audio input. It's all done for you! Don't worry about it!

Then finally that event where the transcribed text comes back:

The text is inside of the `SpeechRecognitionEventArgs` object that's passed in. It has a `Result.Text` property.

The text that comes back is in nice bite size chunks. In other words - it won't return tons and tons of sentences all at once. The SDK is smart enough to send audio up to Azure in small batches to help speed the processing.

There's also a little bit of bingo logic going on here. First I'm taking that string of text and then looping through each character in it - looking for numbers. 

The text that comes back will be full sentences and if people are talking, the bingo numbers will be buried within.

I found that any **G** bingo numbers come across as **3**. So **G56** interprets as **356**. So I'm actually looking for 3 numbers in a row - and if I find 3 then throwing that first one out.

## Bingo!!

That's all there is to it. A pretty simple project that adds some really neat functionality with not a lot of code.

And the best part - it really wows your family! 🤣

So check out the code - and expand upon it - I'd love to see what you come up with! (One suggestion - have it check for & then launch your favorite music streaming service ... that way you can have it play a victory song when you win!)

**AND DON'T JUDGE MY GAME PROGRAMMING SKILLS!** I brute forced everything! 

Multiple arrays for each column - check the winning scenarios manually, and it's easy to trick - it's all hard-coded and brute force!! 💪

And the user interface is U.G.L.Y - ugly!

I mean - if I don't have time to play bingo properly - I don't have time to code a game properly either!!

_(Seriously though - I'm just joking - of course I played ~for real~ during family game nights. The app was just a fun little way of showing off how cool Azure is to my family!)_
