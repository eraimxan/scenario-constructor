using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScenarioKit.Demo.Editor
{
    /// <summary>Creates the demo scenario assets: actions, state, 12 steps, rules, analyzers, loggers, modes.</summary>
    sealed class DemoAssets
    {
        public const string Root = "Assets/Demo/Scenario";

        public readonly Dictionary<string, ActionKey> Actions = new Dictionary<string, ActionKey>();
        public StateKey PowerOn;
        public ScenarioDefinition Scenario;
        public readonly List<LoggerAsset> Loggers = new List<LoggerAsset>();
        public readonly List<ResultAnalyzer> Analyzers = new List<ResultAnalyzer>();

        public static DemoAssets Create()
        {
            AssetDatabase.DeleteAsset(Root);
            foreach (var folder in new[] { "Actions", "States", "Steps", "Modes", "Rules", "Analyzers", "Loggers" })
                Directory.CreateDirectory(Path.Combine(Root, folder));
            AssetDatabase.Refresh();

            var assets = new DemoAssets();
            assets.CreateActions();
            assets.CreateScenario(assets.CreateSteps());
            AssetDatabase.SaveAssets();
            return assets;
        }

        void CreateActions()
        {
            void Action(string id, string title)
            {
                var key = Save<ActionKey>("Actions", id);
                key.Configure(title);
                EditorUtility.SetDirty(key);
                Actions.Add(id, key);
            }

            Action("btn.check", "Кнопка «Проверка связи»");
            Action("btn.start", "Кнопка «Пуск»");
            Action("btn.report", "Кнопка «Отчёт»");
            Action("btn.confirm", "Кнопка «Подтвердить»");
            Action("switch.on", "Рубильник включён");
            Action("switch.off", "Рубильник выключен");
            Action("zone.a", "Вход в зону A");
            Action("zone.b", "Вход в зону B");
            Action("zone.c", "Вход в зону C");
            Action("zone.danger", "Вход в опасную зону");
            Action("zone.base", "Вход на базу");
            Action("dock.box", "Ящик в зоне погрузки");
            Action("pillar.touch", "Касание стойки");
            Action("terminal.touch", "Касание терминала");

            PowerOn = Save<StateKey>("States", "PowerOn");
            SetString(PowerOn, "displayName", "Питание");
        }

        StepDefinition[] CreateSteps()
        {
            var s01 = Step("S01_CheckLink", "Проверка связи", "Нажмите «Проверка связи» на пульте.", "btn.check");
            var s02 = Step("S02_PowerOn", "Включить питание", "Включите рубильник на пульте.", "switch.on");
            s02.AddForbidden(Actions["btn.start"], "«Пуск» до включения питания");
            var s03 = Step("S03_InspectA", "Осмотр зоны A", "Зайдите в жёлтую зону A. Не заходите в красную опасную зону.", "zone.a");
            s03.AddForbidden(Actions["zone.danger"], "вход в опасную зону");
            var s04 = Step("S04_TouchPillar", "Проверка стойки", "Подойдите вплотную к серой контрольной стойке.", "pillar.touch");
            var s05 = Step("S05_RouteBC", "Обход B → C", "Пройдите маршрут: сначала синяя зона B, затем зелёная зона C.", "zone.b", "zone.c");
            var s06 = Step("S06_StartPump", "Запуск насоса", "Нажмите «Пуск». Питание должно быть включено.", "btn.start");
            s06.AddRequirement(PowerOn, true, "нельзя запускать насос без питания");
            var s07 = Step("S07_PushBox", "Погрузка", "Затолкайте коричневый ящик в оранжевую зону погрузки.", "dock.box");
            var s08 = Step("S08_Report", "Отчёт", "Нажмите «Отчёт» на пульте.", "btn.report");
            var s09 = Step("S09_PowerOff", "Выключить питание", "Выключите рубильник.", "switch.off");
            var s10 = Step("S10_ReturnBase", "Возврат на базу", "Вернитесь в серо-голубую зону базы, обходя опасную зону.", "zone.base");
            s10.AddForbidden(Actions["zone.danger"], "вход в опасную зону");
            var s11 = Step("S11_Confirm", "Подтверждение", "Нажмите «Подтвердить». Питание должно быть выключено.", "btn.confirm");
            s11.AddRequirement(PowerOn, false, "сначала выключите питание");
            var s12 = Step("S12_Terminal", "Отметка у терминала", "Коснитесь синего терминала в правом нижнем углу.", "terminal.touch");
            return new StepDefinition[] { s01, s02, s03, s04, s05, s06, s07, s08, s09, s10, s11, s12 };
        }

        ActionSequenceStep Step(string file, string title, string instruction, params string[] sequence)
        {
            var step = Save<ActionSequenceStep>("Steps", file);
            step.Describe(file.Substring(0, 3), title, instruction);
            var keys = new List<ActionKey>();
            foreach (var id in sequence)
                keys.Add(Actions[id]);
            step.Configure(keys);
            EditorUtility.SetDirty(step);
            return step;
        }

        void CreateScenario(StepDefinition[] s)
        {
            var training = Save<TrainingRules>("Rules", "Rules_Training");
            var exam = Save<ExamRules>("Rules", "Rules_Exam");
            exam.Configure(2);

            var trainingScore = Save<TrainingScoreAnalyzer>("Analyzers", "Analyzer_TrainingScore");
            SetString(trainingScore, "displayName", "Тренировка: 10 / 5 / 0 за попытку");
            var examPenalty = Save<ExamPenaltyAnalyzer>("Analyzers", "Analyzer_ExamPenalty");
            SetString(examPenalty, "displayName", "Экзамен: 100 − штрафы");
            Analyzers.Add(trainingScore);
            Analyzers.Add(examPenalty);

            Loggers.Add(Save<ConsoleLogger>("Loggers", "Logger_Console"));
            Loggers.Add(Save<JsonLinesLogger>("Loggers", "Logger_JsonLines"));

            var trainingMode = Save<ModeProfile>("Modes", "Mode_Training");
            trainingMode.Configure("Тренировка", training, trainingScore, OutOfTurnPolicy.Violation, new[]
            {
                Group("Все шаги",
                    Together("1–4 в любом порядке", s[0], s[1], s[2], s[3]),
                    InOrder("5–8 по очереди", s[4], s[5], s[6], s[7]),
                    Together("9 и 10 вместе", s[8], s[9]),
                    Together("11 и 12 вместе", s[10], s[11]))
            });
            SetString(trainingMode, "description", "Смешанная активация; при нарушении шаг повторяется.");

            var examMode = Save<ModeProfile>("Modes", "Mode_Exam");
            examMode.Configure("Экзамен", exam, examPenalty, OutOfTurnPolicy.Violation, new[]
            {
                Group("Группа 1: подготовка", Together("", s[0], s[1], s[2], s[3])),
                Group("Группа 2: запуск", InOrder("", s[4], s[5], s[6], s[7])),
                Group("Группа 3: завершение", Together("", s[8], s[9]), Together("", s[10], s[11]))
            });
            SetString(examMode, "description", "Три группы; нарушение — Failed; 2 нарушения в группе — остальные шаги группы Skipped.");

            var stepByStep = Save<ModeProfile>("Modes", "Mode_TrainingStepByStep");
            stepByStep.Configure("Тренировка по шагам", training, trainingScore, OutOfTurnPolicy.IgnoreUntilActive, new[]
            {
                Group("Все шаги", InOrder("строго по одному", s))
            });
            SetString(stepByStep, "description", "Шаги открываются по одному; шаг до своей очереди не принимает действия.");

            Scenario = Save<ScenarioDefinition>("", "Scenario_PumpStation");
            Scenario.Configure("Запуск насосной станции", s, new[] { trainingMode, examMode, stepByStep });
            SetString(Scenario, "description", "Демонстрационный сценарий: 12 шагов, 3 режима прохождения.");

            foreach (var asset in new Object[] { exam, trainingMode, examMode, stepByStep, Scenario })
                EditorUtility.SetDirty(asset);
        }

        static StepGroup Group(string name, params Stage[] stages) => new StepGroup { name = name, stages = new List<Stage>(stages) };

        static Stage Together(string note, params StepDefinition[] steps) =>
            new Stage { note = note, activation = StageActivation.Simultaneous, steps = new List<StepDefinition>(steps) };

        static Stage InOrder(string note, params StepDefinition[] steps) =>
            new Stage { note = note, activation = StageActivation.Sequential, steps = new List<StepDefinition>(steps) };

        static T Save<T>(string folder, string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, $"{Root}/{(folder.Length > 0 ? folder + "/" : "")}{name}.asset");
            return asset;
        }

        public static void SetString(Object target, string property, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
