// @tags=test

const arenaTicketCount = getArenaTicketCount();

const ticketResult = macroService.PollPattern(patterns.arena.ticket);
const slashPattern = macroService.ClonePattern(patterns.arena.ticket.slash, { X: ticketResult.Point.X + 60, Width: 100, Padding: 5, PathSuffix: `_x${ticketResult.Point.X}`, OffsetCalcType: 'None' });
const slashResult = macroService.PollPattern(slashPattern);
const valueBounds = {
	X: ticketResult.Point.X + 60,
	Y: slashResult.Point.Y - 9,
	Height: slashPattern.Height + 5,
	Width: slashResult.Point.X - ticketResult.Point.X - 70
};
while (macroService.IsRunning) {
	macroService.DebugRectangle(valueBounds);
	sleep(1_000);
}

arenaTicketCount = macroService.FindTextWithBounds(valueBounds);

return { arenaTicketCount };

//const unitTitleAndName = macroService.FindText(patterns.battle.teamFormation.unitTitleAndName);
//return unitTitleAndName;
