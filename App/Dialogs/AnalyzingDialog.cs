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
		int totalMoveCount = Math.Max(1, game.GetLastNode().Rank + 1);
		int scale = 1000 / totalMoveCount;
		var node = game.GetRootNode();
		var moves = new List<App.TreeNode>();
		var bestMoves = new List<string>();
		var bestScores = new List<int>();
		engine.NewGame();
		engine.SetPosition(game.GetUciPosition(), moves.Select(node => node.Uci).ToArray());
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
						moves.Add(node);
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
						engine.SetPosition(game.GetUciPosition(), moves.Select(node => node.Uci).ToArray());
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

	private void ProcessResults(List<App.TreeNode> moves, List<string> bestMoves, List<int> bestScores)
	{
		_timer.Stop();
		double[] accuracies = { 100, 100 };
		int startingColor = GameManager.GetGame().GetStartingColor();
		for (int i = 0; i < moves.Count; i++)
		{
			int color = startingColor ^ i % 2;
			if (bestMoves[i].StartsWith(moves[i].Uci))
			{
				moves[i].Class = MoveClassifications.Best;
				continue;
			}
			double previousScore = Scores.ToWinProbability(bestScores[i]);
			double currentScore = Scores.ToWinProbability(bestScores[i + 1]);
			double loss = Math.Max(0, color == White ? previousScore - currentScore : currentScore - previousScore);
			accuracies[color] *= 1 - loss / Math.Sqrt(moves.Count);
			moves[i].Class = MoveClassifications.GetClassForWinPercentageLoss(loss);
		}
		PgnManager.SetValue("WhiteAccuracy", accuracies[White].ToString("F1"));
		PgnManager.SetValue("BlackAccuracy", accuracies[Black].ToString("F1"));
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
