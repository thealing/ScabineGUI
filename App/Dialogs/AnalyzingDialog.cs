namespace Scabine.App.Dialogs;

using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Scabine.Core.Pieces;
using static Scabine.App.Dialogs.DialogCreator;
using System.Collections.Generic;
using Scabine.Core;
using System.Runtime.InteropServices;
using System.Xml.Linq;

internal class AnalyzingDialog : BaseDialog
{
	public AnalyzingDialog(IEngine engine, int depth)
	{
		ClientSize = new Size(460, 150);
		Text = "Analyzing game";
		Font = new Font("Segoe UI", 12);
		_progressBar = AddProgressBar(Controls, 30, 20, 400, 40);
		_label = AddLabel(Controls, "", 20, 90, 300, 40);
		AddButton(Controls, "Cancel", 330, 90, 110, 40, Cancel);
		TreeGame game = GameManager.GetGame();
		int totalMoveCount = game.GetLastNode().Rank + 1;
		int scale = 1000 / totalMoveCount;
		var node = game.GetRootNode();
		var moves = new List<string>();
		var bestMoves = new List<string>();
		var bestScores = new List<int>();
		engine.NewGame();
		engine.SetPosition(game.GetUciPosition(), moves.ToArray());
		engine.StartThinking(depth, 0, 0, 0, 0, 0);
		_progressBar.Maximum = totalMoveCount * depth;
		_timer = new Timer { Interval = 10 };
		_timer.Tick += (sender, e) =>
		{
			if (!engine.IsThinking())
			{
				node.Eval = engine.GetBestScore() * (node.Color == White ? -1 : 1);
				bestScores.Add(node.Eval.Value);
				bestMoves.Add(engine.GetBestMoves());
				if (node.Children.Any())
				{
					node = node.Children.First();
					if (node.Move != null)
					{
						moves.Add(node.Move.Value.ToString());
					}
					if (node == game.GetLastNode() && game.GetResult() != Result.Ongoing)
					{
						switch (game.GetResult())
						{
							case Result.WhiteWon:
								bestScores.Add(Scores.MateScore);
								break;
							case Result.BlackWon:
								bestScores.Add(-Scores.MateScore);
								break;
							case Result.Draw:
								bestScores.Add(Scores.DrawScore);
								break;
						}
						ProcessResults(moves, bestMoves, bestScores);
					}
					else
					{
						engine.NewGame();
						engine.SetPosition(game.GetUciPosition(), moves.ToArray());
						engine.StartThinking(depth, 0, 0, 0, 0, 0);
					}
				}
				else
				{
					ProcessResults(moves, bestMoves, bestScores);
				}
			}
			int evaluatedMoveCount = node.Rank + 1;
			_label.Text = $"Moves evaluated: {evaluatedMoveCount} / {totalMoveCount}";
			_progressBar.Value = Math.Min(_progressBar.Maximum, evaluatedMoveCount * depth + engine.GetReachedDepth());
		};
		_timer.Start();
	}

	private void ProcessResults(List<string> moves, List<string> bestMoves, List<int> bestScores)
	{
		_timer.Stop();
		AnalyzisResult[] results = new AnalyzisResult[ColorCount];
		results[White].Accuracy = 1;
		results[Black].Accuracy = 1;
		int startingColor = GameManager.GetGame().GetStartingColor();
		for (int i = 0; i < moves.Count; i++)
		{
			int color = startingColor ^ i % 2;
			if (bestMoves[i].StartsWith(moves[i]))
			{
				results[color].Best++;
				continue;
			}
			double previousScore = Scores.ToWinProbability(bestScores[i]);
			double currentScore = Scores.ToWinProbability(bestScores[i + 1]);
			double loss = Math.Max(0, color == White ? previousScore - currentScore : currentScore - previousScore);
			results[color].Accuracy *= 1 - loss / Math.Sqrt(moves.Count);
			if (loss >= 0.25)
			{
				results[color].Blunder++;
			}
			else if (loss >= 0.15)
			{
				results[color].Mistake++;
			}
			else if (loss >= 0.08)
			{
				results[color].Inaccuracy++;
			}
			else if (loss >= 0.02)
			{
				results[color].Good++;
			}
			else
			{
				results[color].Great++;
			}
		}
		PgnManager.SetValue("WhiteAR", AnalyzisResult.EncodeToBase64(results[White]));
		PgnManager.SetValue("BlackAR", AnalyzisResult.EncodeToBase64(results[Black]));
		MessageBeep(0);
		Close();
	}

	private void Cancel(object? sender, EventArgs e)
	{
		_timer.Stop();
		Close();
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		_timer.Dispose();
		base.OnFormClosed(e);
	}

	private readonly ProgressBar _progressBar;
	private readonly Label _label;
	private readonly Timer _timer;

	[DllImport("user32.dll")]
	public static extern bool MessageBeep(uint uType);
}
