using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KMHelper;
using UnityEngine;
using Random = UnityEngine.Random;

public class timekeeper : MonoBehaviour {
    private static int _moduleIdCounter = 1;
    private readonly bool _isSolved = false;
    private bool _lightsOn;
    private int _moduleId;
    private int batteryCount;
    private int batteryHolderCount;
    private readonly int black = 5;
    private readonly int blue = 1;

    public GameObject colorblindObj;
    public string[] colorNames = new string[6]; //To make debug messages easier

    public Material[] colors = new Material[6]; //0=red,1=blue,2=yellow,3=green,4=white,5=black

    private int correctLEDIndex;
    private int correctTime;
    private int displayedNumber;
    public TextMesh displayedNumberText, colorblindTextOne, colorblindTextTwo, colorblindTextThree, colorblindTextScreen;
    private int displayedTextColor;

    private bool end;
    private readonly int green = 3;
    public KMBombInfo info;

    public KMSelectable LEDOne;

    private List<int> correctTimes = new List<int>();

    private int LEDOneColor;

    public MeshRenderer LEDOneMesh;
    private KMSelectable[] LEDs;
    public KMSelectable LEDThree;
    private int LEDThreeColor;
    public MeshRenderer LEDThreeMesh;
    public KMSelectable LEDTwo;
    private int LEDTwoColor;
    public MeshRenderer LEDTwoMesh;
    private int[] letterIndexes;

    private char[] letters;
    private int litCount;
    public KMBombModule module;

    private int month;
    public KMAudio newAudio;
    private int[] numbers;
    private int portCount;
    private int portPlateCount;

    private readonly int red = 0;

    private readonly Color redC = new Color(255, 0, 0);
    private readonly Color greenC = new Color(0, 255, 0);
    private readonly Color blueC = new Color(0, 0, 255);
    private readonly Color yellowC = new Color(255, 255, 0);
    private readonly Color whiteC = new Color(255, 255, 255);

    private bool rule9;
    private readonly Color[] textColors = new Color[5]; //0=red,1=blue,2=yellow,3=green,4=white


    private bool TwitchPlaysSkipTimeAllowed = true;

    private bool TwitchZenMode;
    private int unlitCount;
    private readonly int white = 4;
    private readonly int yellow = 2;

    private void Start() {
        _moduleId = _moduleIdCounter++;
        module.OnActivate += Activate;
    }

    private void Awake() {
        LEDOne.OnInteract += delegate {
            handleLEDPress(0);
            return false;
        };
        LEDTwo.OnInteract += delegate {
            handleLEDPress(1);
            return false;
        };
        LEDThree.OnInteract += delegate {
            handleLEDPress(2);
            return false;
        };
    }

    private void handleLEDPress(int index) {
        var timeLeft = (int) info.GetTime();
        
        newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, LEDs[index].transform);
        if (_isSolved || !_lightsOn) return;
        if (index == correctLEDIndex) {
            foreach (int time in correctTimes) {
                if (Math.Abs(timeLeft - time) <= 2) {
                    if (timeLeft > 10) {
                        newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, LEDs[index].transform);
                    } else {
                        module.HandleStrike();
                        Debug.LogFormat("[TimeKeeper #{0}] Remember, this module does not like to wait. Since you pressed the correct answer with less than ten seconds remaining, you received a strike before the module got solved.", _moduleId);
                    }

                    module.HandlePass();
                    return;
                }
            }

            Debug.LogFormat("[TimeKeeper #{0}] Correct LED pressed at wrong time. Expected times: {1}. Received time: {2}.", _moduleId, string.Join(", ", correctTimes.Select((x) => x.ToString()).ToArray()), timeLeft);
            Debug.LogFormat("[TimeKeeper #{0}] If you feel that this strike is an error, please don't hesitate to contact @AAces#0908 on discord with a copy of this log file so we can get this sorted out.", _moduleId);
            module.HandleStrike();
            
        } else {
            Debug.LogFormat("[TimeKeeper #{0}] Incorrect LED pressed. Expected {1}. Received: {2}.", _moduleId, correctLEDIndex + 1, index + 1);
            Debug.LogFormat("[TimeKeeper #{0}] If you feel that this strike is an error, please don't hesitate to contact @AAces#0908 on discord with a copy of this log file so we can get this sorted out.", _moduleId);
            module.HandleStrike();
        }
    }

    private void Activate() {
        Init();
        _lightsOn = true;
    }

    private void Init() {
        correctLEDIndex = -1;
        LEDs = new KMSelectable[] {LEDOne, LEDTwo, LEDThree};
        letters = info.GetSerialNumberLetters().ToArray();
        numbers = info.GetSerialNumberNumbers().ToArray();
        batteryCount = info.GetBatteryCount();
        batteryHolderCount = info.GetBatteryHolderCount();
        portPlateCount = info.GetPortPlateCount();
        portCount = info.GetPortCount();
        litCount = info.GetOnIndicators().Count();
        unlitCount = info.GetOffIndicators().Count();
        letterIndexes = new int[letters.Length];
        for (var i = 0; i < letterIndexes.Length; i++) letterIndexes[i] = GetIndexInAlphabet(letters[i]);
        displayedNumber = Random.Range(1, 51);
        var text = displayedNumber.ToString();
        if (displayedNumber < 10)
            displayedNumberText.text = "0" + text;
        else
            displayedNumberText.text = text;
        setupColors();
        month = DateTime.Today.Month;
        Debug.LogFormat("[TimeKeeper #{0}] Displayed Number: {5} in {4}. LED One color: {1}. LED Two color: {2}. LED Three color: {3}.", _moduleId, colorNames[LEDOneColor], colorNames[LEDTwoColor], colorNames[LEDThreeColor], colorNames[displayedTextColor], displayedNumber);
        getCorrectAnswer();
    }

    private void setupTextColors() {
        textColors[0] = new Color(255, 0, 0);
        textColors[1] = new Color(0, 0, 255);
        textColors[2] = new Color(255, 255, 0);
        textColors[3] = new Color(0, 255, 0);
        textColors[4] = new Color(255, 255, 255);
    }

    private void setupColors() {
        var rndColor = Random.Range(0, 5);
        setupTextColors();
        displayedNumberText.color = textColors[rndColor];
        displayedTextColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDOneMesh.material = colors[rndColor];
        LEDOneColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDTwoMesh.material = colors[rndColor];
        LEDTwoColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDThreeMesh.material = colors[rndColor];
        LEDThreeColor = rndColor;

        switch (LEDOneColor) //0=red,1=blue,2=yellow,3=green,4=white,5=black
        {
            case 0:
                colorblindTextOne.color = redC;
                colorblindTextOne.text = "R";
                break;
            case 1:
                colorblindTextOne.color = blueC;
                colorblindTextOne.text = "Blu";
                break;
            case 2:
                colorblindTextOne.color = yellowC;
                colorblindTextOne.text = "Y";
                break;
            case 3:
                colorblindTextOne.color = greenC;
                colorblindTextOne.text = "G";
                break;
            case 4:
                colorblindTextOne.color = whiteC;
                colorblindTextOne.text = "W";
                break;
            case 5:
                colorblindTextOne.color = whiteC;
                colorblindTextOne.text = "Bla";
                break;
        }

        switch (LEDTwoColor) //0=red,1=blue,2=yellow,3=green,4=white,5=black
        {
            case 0:
                colorblindTextTwo.color = redC;
                colorblindTextTwo.text = "R";
                break;
            case 1:
                colorblindTextTwo.color = blueC;
                colorblindTextTwo.text = "Blu";
                break;
            case 2:
                colorblindTextTwo.color = yellowC;
                colorblindTextTwo.text = "Y";
                break;
            case 3:
                colorblindTextTwo.color = greenC;
                colorblindTextTwo.text = "G";
                break;
            case 4:
                colorblindTextTwo.color = whiteC;
                colorblindTextTwo.text = "W";
                break;
            case 5:
                colorblindTextTwo.color = whiteC;
                colorblindTextTwo.text = "Bla";
                break;
        }

        switch (LEDThreeColor) //0=red,1=blue,2=yellow,3=green,4=white,5=black
        {
            case 0:
                colorblindTextThree.color = redC;
                colorblindTextThree.text = "R";
                break;
            case 1:
                colorblindTextThree.color = blueC;
                colorblindTextThree.text = "Blu";
                break;
            case 2:
                colorblindTextThree.color = yellowC;
                colorblindTextThree.text = "Y";
                break;
            case 3:
                colorblindTextThree.color = greenC;
                colorblindTextThree.text = "G";
                break;
            case 4:
                colorblindTextThree.color = whiteC;
                colorblindTextThree.text = "W";
                break;
            case 5:
                colorblindTextThree.color = whiteC;
                colorblindTextThree.text = "Bla";
                break;
        }

        switch (displayedTextColor) //0=red,1=blue,2=yellow,3=green,4=white
        {
            case 0:
                colorblindTextScreen.color = redC;
                colorblindTextScreen.text = "Red";
                break;
            case 1:
                colorblindTextScreen.color = blueC;
                colorblindTextScreen.text = "Blue";
                break;
            case 2:
                colorblindTextScreen.color = yellowC;
                colorblindTextScreen.text = "Yellow";
                break;
            case 3:
                colorblindTextScreen.color = greenC;
                colorblindTextScreen.text = "Green";
                break;
            case 4:
                colorblindTextScreen.color = whiteC;
                colorblindTextScreen.text = "White";
                break;
        }

        if (GetComponent<KMColorblindMode>().ColorblindModeActive) {
            colorblindObj.SetActive(true);
            Debug.LogFormat("[TimeKeeper #{0}] Colorblind mode enabled.", _moduleId);
        } else {
            colorblindObj.SetActive(false);
        }
    }

    private void getCorrectAnswer() {
        for (var i = 1; i < 22; i++) {
            if (end) break;
            steps(i);
        }

        if (correctTime < 0) {
            correctTime *= -1;
            Debug.LogFormat("[TimeKeeper #{0}] Correct time less than 0, multiplying by -1.", _moduleId);
        }

        if (correctTime < 10) {
            correctTime += 13;
            Debug.LogFormat("[TimeKeeper #{0}] Correct time less than 10, adding 13.", _moduleId);
        }

        if (correctLEDIndex == -1) getCorrectLED();
        Debug.LogFormat("[TimeKeeper #{0}] Correct LED: {1}. Correct time (in seconds): {2}.", _moduleId, correctLEDIndex + 1, correctTime);

        GetMultiplesOfTime();
    }

    void GetMultiplesOfTime() {
        correctTimes.Add((int)Math.Floor(correctTime * Math.Pow(2, 14)));
        for (int i = 13; correctTimes.Last() > 1; i--) {
            correctTimes.Add((int)Math.Floor(correctTime * Math.Pow(2, i)));
        }
    }

    private void steps(int step) {
        switch (step) {
            case 1:
                correctTime = displayedNumber;
                for (var i = 0; i < letterIndexes.Length; i++) correctTime += letterIndexes[i];
                for (var n = 0; n < numbers.Length; n++) correctTime -= numbers[n];
                Debug.LogFormat("[TimeKeeper #{0}] Rule 1 used.", _moduleId);

                break;
            case 2:
                if (LEDOneColor == white) {
                    correctTime += 14;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 2 used.", _moduleId);
                }

                break;
            case 3:
                if (LEDTwoColor == displayedTextColor) {
                    correctTime += 22;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 3 = true.", _moduleId);
                } else {
                    correctTime += 13;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 3 = false.", _moduleId);
                }

                break;
            case 4:
                correctTime += 2 * portPlateCount;
                Debug.LogFormat("[TimeKeeper #{0}] Rule 4 added " + 2 * portPlateCount, _moduleId);
                if (info.IsPortPresent(KMBombInfoExtensions.KnownPortType.DVI)) {
                    correctTime -= 9;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 4: DVI found.", _moduleId);
                }

                break;
            case 5:
                if (LEDOneColor == LEDTwoColor && LEDOneColor == LEDThreeColor) {
                    setCorrectLED(0);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 5 used.", _moduleId);
                }

                break;
            case 6:
                if ((displayedTextColor == red || displayedTextColor == green || displayedTextColor == blue) && LEDOneColor != yellow && LEDTwoColor != yellow && LEDThreeColor != yellow) {
                    correctTime += displayedNumber;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 6 used.", _moduleId);
                }

                break;
            case 7:
                if (info.GetSolvableModuleNames().Count() > batteryCount + batteryHolderCount) {
                    correctTime -= 18;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 7 used.", _moduleId);
                }

                break;
            case 8:
                if (correctTime % 2 == 0 && correctTime > 72) {
                    correctTime /= 2;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 8 used.", _moduleId);
                }

                break;
            case 9:
                if (LEDTwoColor == green || LEDTwoColor == black) {
                    setCorrectLED(1);
                    rule9 = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 9 used.", _moduleId);
                }

                break;
            case 10:
                if (mod23() < 2 * portCount) {
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 10 used.", _moduleId);
                }

                break;
            case 11:
                correctTime += month;
                Debug.LogFormat("[TimeKeeper #{0}] Rule 11 used.", _moduleId);
                break;
            case 12:
                if (displayedNumber > 23) {
                    correctTime += batteryHolderCount;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 12 = true.", _moduleId);
                } else {
                    correctTime *= batteryHolderCount;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 12 = false.", _moduleId);
                }

                break;
            case 13:
                correctTime += 2 * litCount;
                correctTime -= 3 * unlitCount;
                Debug.LogFormat("[TimeKeeper #{0}] Rule 13 added " + 2 * litCount + " and subtracted " + 3 * unlitCount, _moduleId);
                break;
            case 14:
                if (LEDOneColor == displayedTextColor && LEDOneColor == LEDThreeColor && LEDOneColor != LEDTwoColor) {
                    setCorrectLED(2);
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 14 used.", _moduleId);
                }

                break;
            case 15:
                if (rule9) {
                    correctTime += 10;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 15 = true.", _moduleId);
                } else {
                    correctTime -= 19;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 15 = false.", _moduleId);
                }

                break;
            case 16:
                if (correctTime < 0) {
                    correctTime *= -2;
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 16 used.", _moduleId);
                }

                break;
            case 17:
                correctTime *= 3;
                Debug.LogFormat("[TimeKeeper #{0}] Rule 17 used.", _moduleId);
                break;
            case 18:
                if (colorNames[LEDOneColor].Length + colorNames[LEDTwoColor].Length + colorNames[LEDThreeColor].Length > 13) {
                    correctTime += colorNames[displayedTextColor].Length;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 18 used.", _moduleId);
                }

                break;
            case 19:
                if (portPlateCount == 0) {
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 19 used.", _moduleId);
                }

                break;
            case 20:
                if (info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.FRK)) {
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 FRK out used.", _moduleId);
                    break;
                }

                if (colorNames[LEDOneColor].Length > colorNames[LEDTwoColor].Length && colorNames[LEDOneColor].Length > colorNames[LEDThreeColor].Length) {
                    setCorrectLED(0);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 LED 1 used.", _moduleId);
                } else if (colorNames[LEDTwoColor].Length > colorNames[LEDOneColor].Length && colorNames[LEDTwoColor].Length > colorNames[LEDThreeColor].Length) {
                    setCorrectLED(1);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 LED 2 used.", _moduleId);
                } else if (colorNames[LEDThreeColor].Length > colorNames[LEDTwoColor].Length && colorNames[LEDThreeColor].Length > colorNames[LEDOneColor].Length) {
                    setCorrectLED(2);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 LED 3 used.", _moduleId);
                } else {
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 Tie", _moduleId);
                }

                break;
            case 21:
                if (unlitCount == 0) {
                    correctTime *= 3;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 21 no unlit.", _moduleId);
                    break;
                }

                foreach (var label in info.GetOffIndicators()) {
                    correctTime += GetIndexInAlphabet(label[0]);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 21: added " + label, _moduleId);
                }

                break;
        }
    }

    private void getCorrectLED() {
        if (correctTime < 100)
            correctLEDIndex = 0;
        else if (displayedTextColor == green && LEDOneColor != green)
            correctLEDIndex = 2;
        else if (LEDOneColor != LEDTwoColor && LEDOneColor != LEDThreeColor && LEDOneColor != displayedTextColor && LEDTwoColor != LEDThreeColor && LEDTwoColor != displayedTextColor && LEDThreeColor != displayedTextColor)
            correctLEDIndex = 0;
        else if (info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Parallel))
            correctLEDIndex = 1;
        else
            correctLEDIndex = 2;
    }

    private static int GetIndexInAlphabet(char value) {
        switch (value) {
            case 'A':
                return 1;
            case 'B':
                return 2;
            case 'C':
                return 3;
            case 'D':
                return 4;
            case 'E':
                return 5;
            case 'F':
                return 6;
            case 'G':
                return 7;
            case 'H':
                return 8;
            case 'I':
                return 9;
            case 'J':
                return 10;
            case 'K':
                return 11;
            case 'L':
                return 12;
            case 'M':
                return 13;
            case 'N':
                return 14;
            case 'O':
                return 15;
            case 'P':
                return 16;
            case 'Q':
                return 17;
            case 'R':
                return 18;
            case 'S':
                return 19;
            case 'T':
                return 20;
            case 'U':
                return 21;
            case 'V':
                return 22;
            case 'W':
                return 23;
            case 'X':
                return 24;
            case 'Y':
                return 25;
            case 'Z':
                return 26;
            default:
                return 0;
        }
    }

    int mod23() {
        if (correctTime < 0) {
            return (correctTime % 23 + 23) % 23;
        } else {
            return correctTime % 23;
        }
    }

    private void setCorrectLED(int index) {
        if (correctLEDIndex == -1) correctLEDIndex = index;
    }

#pragma warning disable 414

    private string TwitchHelpMessage = "Press the second LED at 3m14s with !{0} press 2 at 3:14. To submit a time of 43 seconds, use '0:43', not just '43', and to submit a time of 2 hours, 52 minutes, and 48 seconds, use '172:52'. Use !{0} colorblind to enable colorblind mode.";

#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string input) {
        var rgx = new Regex(@"^press [123] (at|on) [0-9]?[0-9]?[0-9]:[0-5][0-9]$");
        if (rgx.IsMatch(input)) {
            var split = input.ToLowerInvariant().Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            var time = split[3].ToLowerInvariant().Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);
            var led = int.Parse(split[1]) - 1;
            var seconds = 60 * int.Parse(time[0]) + int.Parse(time[1]);
            yield return null;

            if (!TwitchZenMode) {
                if (Mathf.FloorToInt(info.GetTime()) < seconds) yield break;
            } else {
                if (Mathf.FloorToInt(info.GetTime()) > seconds) yield break;
            }

            var timeToSkipTo = seconds;
            var music = false;
            if (TwitchZenMode) {
                timeToSkipTo = seconds - 5;
                if (seconds - info.GetTime() > 15) yield return "skiptime " + timeToSkipTo;
                if (seconds - info.GetTime() > 10) music = true;
            } else {
                timeToSkipTo = seconds + 5;
                if (info.GetTime() - seconds > 15) yield return "skiptime " + timeToSkipTo;
                if (info.GetTime() - seconds > 10) music = true;
            }

            if (music) yield return "waiting music";
            while (Mathf.FloorToInt(info.GetTime()) != seconds) yield return "trycancel LED wasn't pressed due to request to cancel.";
            if (music) yield return "end waiting music";
            handleLEDPress(led);
        } else if (input.ToLowerInvariant().Equals("colorblind")) {
            yield return null;
            colorblindObj.SetActive(true);
            Debug.LogFormat("[TimeKeeper #{0}] Colorblind mode enabled via TP command.", _moduleId);
        }
    }
}