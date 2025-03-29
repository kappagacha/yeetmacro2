const popupPatterns = [patterns.lobby.expedition, patterns.general.tapEmptySpace, settings.goToLobby.userClickPattern.Value, patterns.general.exitCheckIn, patterns.general.startMessageClose];
const loopPatterns = [patterns.lobby.level, patterns.titles.guildHallOfHonor, patterns.titles.guild, ...popupPatterns];

macroService.PollPattern(loopPatterns, { DoClick: true, PredicatePattern: patterns.guild.hallOfHonor.recieveMessage });