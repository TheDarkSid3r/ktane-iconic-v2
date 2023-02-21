using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public sealed class IconicModule : MonoBehaviour
{
    [SerializeField]
    private KMBombModule _module;

    [SerializeField]
    private KMSelectable _selectable;

    [SerializeField]
    private KMBombInfo _bombInfo;

    [SerializeField]
    private KMBossModule _boss;

    [SerializeField]
    private KMAudio _audio;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private CanvasGroup _pixelsGroup;

    [SerializeField]
    private Text _displayText;

    [SerializeField]
    private PixelGrid _pixelGrid;

    [SerializeField]
    private RawImage _iconImage;

    [SerializeField]
    private Text _blankText;

    [SerializeField]
    private Texture2D _emptyIcon;

    [SerializeField]
    private Texture2D _blan;

    private static int _moduleIdCounter = 0;
    private static readonly string[] _coordinateLetters = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF" };

    private int _moduleId = 0;
    private bool _isFocused = false;
    private bool _isReady = false;
    private bool _isSolved = false;
    private bool _hasAddedIgnoredModules = false;
    private string[] _ignoreList;
    private readonly Queue<string> _queue = new Queue<string>();
    private readonly List<string> _solves = new List<string>();
    private IconData _currentIcon;
    private IconPartData _currentPart;

    private string[] NonIgnoredBombModules
    {
        get
        {
            return _bombInfo.GetSolvableModuleNames().Where(name => !_ignoreList.Contains(name)).ToArray();
        }
    }

    private string[] IgnoredBombModules
    {
        get
        {
            return _bombInfo.GetSolvableModuleNames().Where(_ignoreList.Contains).ToArray();
        }
    }

    private string[] NonIgnoredSolves
    {
        get
        {
            return _solves.Where(name => !_ignoreList.Contains(name)).ToArray();
        }
    }

    private void Awake()
    {
        _moduleId = ++_moduleIdCounter;
    }

    private void Log(string format, params object[] args)
    {
        Debug.LogFormat("[Iconic #{0}] {1}", _moduleId, string.Format(format, args));
    }

    private void Start()
    {
        _blankText.enabled = false;
        UpdateDisplay();
        _ignoreList = _boss.GetIgnoredModules("Iconic", new[] { "+", "14", "A>N<D", "Black Arrows", "Brainf---", "Busy Beaver", "Cube Synchronization", "Don't Touch Anything", "Floor Lights", "Forget Everything", "Forget Any Color", "Forget Enigma", "Forget It Not", "Forget Maze Not", "Forget Infinity", "Forget Me Later", "Forget Me Not", "Forget Perspective", "Forget The Colors", "Forget Them All", "Forget This", "Forget Us Not", "Gemory", "Iconic", "Keypad Directionality", "Kugelblitz", "OmegaForget", "Organization", "Out of Time", "Purgatory", "RPS Judging", "Security Council", "Shoddy Chess", "Simon Forgets", "Simon's Stages", "Soulscream", "Souvenir", "Tallordered Keys", "Tetrahedron", "The Board Walk", "The Twin", "The Very Annoying Button", "Ultimate Custom Night", "Whiteout", "Übermodule", "Bamboozling Time Keeper", "Doomsday Button", "OmegaDestroyer", "Password Destroyer", "The Time Keeper", "Timing is Everything", "Turn The Key", "Zener Cards" });
        _module.OnActivate += OnActivate;
        _selectable.OnInteract += OnInteract;
        _selectable.OnDefocus += OnDefocus;
        _pixelGrid.OnPixelSelected += OnPixelSelected;
    }

    private void OnActivate()
    {
        StartCoroutine(Activation());
    }

    private IEnumerator Activation()
    {
        yield return new WaitUntil(() => IconicData.Instance.RemoteCheckComplete);

        List<string> modules = _bombInfo.GetModuleNames(),
            solvable = _bombInfo.GetSolvableModuleNames();

        foreach (string name in modules)
        {
            // it's needy!! let's add it
            if (!solvable.Contains(name))
                Enqueue(name);
        }

        CheckQueue();

        _isReady = true;
    }

    private bool OnInteract()
    {
        _isFocused = true;
        UpdateCanvasGroups();
        return true;
    }

    private void OnDefocus()
    {
        _isFocused = false;
        UpdateCanvasGroups();
    }

    private void UpdateCanvasGroups()
    {
        _canvasGroup.blocksRaycasts = _isFocused;
        _pixelsGroup.blocksRaycasts = _currentIcon != null;
    }

    private void UpdateDisplay()
    {
        if (_currentIcon != null && _currentPart != null)
        {
            _displayText.text = _currentPart.Name;
            _iconImage.texture = IconicData.Instance.IconSprite;
            float x = 32f / IconicData.Instance.IconSprite.width, y = 32f / IconicData.Instance.IconSprite.height;
            _iconImage.uvRect = new Rect(x * _currentIcon.LocationX, y - y * (_currentIcon.LocationY + 2), x, y);
        }
        else
        {
            _displayText.text = _isSolved ? "GG!" : "Iconic";
            _iconImage.texture = _isSolved ? _blan : _emptyIcon;
            _iconImage.uvRect = new Rect(0, 0, 1, 1);
        }

        UpdateCanvasGroups();
    }

    private void Update()
    {
        if (!_isReady || _hasAddedIgnoredModules || _isSolved)
            return;

        List<string> solves = _bombInfo.GetSolvedModuleNames();
        if (solves.Count <= _solves.Count)
            return;

        foreach (string solve in _solves)
            solves.Remove(solve);

        _solves.AddRange(solves);
        foreach (string solve in solves)
        {
            if (_ignoreList.Contains(solve))
            {
                Log("Ignored module solved: {0}", solve);
            }
            else
            {
                Enqueue(solve);
            }
        }
    }

    private void Enqueue(string name)
    {
        _queue.Enqueue(name);
        CheckQueue();
    }

    private void CheckQueue()
    {
        if (_queue.Count < 1 || _currentIcon != null)
            return;

        string next = _queue.Dequeue();
        IconData icon = IconicData.Instance.GetIcon(next);
        if (icon == null)
        {
            _blankText.enabled = true;
            _blankText.text = next;
            icon = IconicData.Instance.GetIcon("Blank");
            if (icon == null)
            {
                Log("Could not find an icon for \"{0}\" and could not find the Blank icon, moving on to next solve if applicable", next);
                CheckQueue();
                return;
            }

            Log("Could not find an icon for \"{0}\", using Blank", next);
        }

        _currentIcon = icon;
        _currentPart = _currentIcon.Parts.PickRandom();
        UpdateDisplay();
    }

    private bool IsPixelCorrect(int index)
    {
        if (_currentIcon == null || _currentPart == null) return false;
        return _currentPart.Indices.Contains(index);
    }

    private void OnPixelSelected(int index)
    {
        if (_currentIcon == null || _currentPart == null) return;

        bool isCorrect = IsPixelCorrect(index);
        Log("{0} part of {1} selected, \"{2}\" {3} ({4}, {5})", isCorrect ? "Correct" : "Incorrect", _currentIcon.Name, _currentPart.Name, isCorrect ? "is at" : "is not at", index % 32 + 1, Mathf.FloorToInt(index / 32) + 1);
        if (isCorrect)
        {
            _audio.PlaySoundAtTransform("Blip", transform);
        }
        else
        {
            Log("Strike!");
            _module.HandleStrike();
        }

        _currentIcon = null;
        _currentPart = null;
        _blankText.enabled = false;
        UpdateDisplay();

        if (_queue.Count < 1)
        {
            if (!_isSolved && NonIgnoredSolves.Length >= NonIgnoredBombModules.Length)
            {
                if (!_hasAddedIgnoredModules)
                {
                    bool hasSkippedIconic = false;
                    foreach (string name in IgnoredBombModules)
                    {
                        if (name == _module.ModuleDisplayName && !hasSkippedIconic)
                            hasSkippedIconic = true;

                        else
                            Enqueue(name);
                    }

                    _hasAddedIgnoredModules = true;
                    return;
                }

                Log("Module solved");
                _isSolved = true;
                _module.HandlePass();
                UpdateDisplay();
                _audio.PlaySoundAtTransform("GoodGame", transform);
            }

            return;
        }

        CheckQueue();
    }

    private int GetIndex(string twitchCoord)
    {
        string coord = twitchCoord.ToUpper();
        for (int alpha = 0; alpha < _coordinateLetters.Length; alpha++)
            if (coord.StartsWith(_coordinateLetters[alpha]))
            {
                int num;
                if (int.TryParse(coord.Substring(_coordinateLetters[alpha].Length), out num) && num >= 1 && num <= 32)
                    return (num - 1) * 32 + alpha;
            }

        return -1;
    }

    private readonly string TwitchHelpMessage = @"!{0} press <coord> [Presses the pixel as the specified coordinate] | Valid coordinates are A1-AF32 (after Z it becomes AA)";

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int index = GetIndex(parameters[1]);
                if (index > -1)
                {
                    bool correct = IsPixelCorrect(index);
                    OnPixelSelected(index);
                    if (correct)
                        yield return "awardpoints 1";
                }
                else
                {
                    yield return string.Format("sendtochaterror The specified coordinate of the pixel you wish to press (\"{0}\") is invalid!", parameters[1]);
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the coordinate of the pixel you wish to press!";
            }
            yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!_isSolved)
        {
            while (_currentIcon == null || _currentPart == null)
                yield return true;

            OnPixelSelected(_currentPart.Indices[0]);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
