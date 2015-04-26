	package bots;

import java.util.*;

import pirates.game.*;

public class MyBot implements PirateBot{

	private Manager manager = null;
    private boolean inited = false;
    public static PirateGame game;
    public static Enemy myEnemy = null;
    public static HashSet<Integer> islandsScored=new HashSet<Integer>();
    public static ArrayList<Direction> dirList=new ArrayList<Direction>();
	public static int cargoScore=0;
	public static GameMode gameMode;
	public static TargetBank gameTargetBank = null;
	public static Sailing sailor=null;
	public static List<Location> islandLocations=new ArrayList<Location>();
    
    @Override
	public void doTurn(PirateGame state) {
		game = state;
		initialization();
		// Temporarily code for one island, just to get to the next level.
//		if(game.islands().size()==1) {
//			oneIslandTactic();
//			
//		}
//		else {
			if(!inited) 
	        {
				dirList.add(Direction.NORTH);
				dirList.add(Direction.EAST);
				dirList.add(Direction.SOUTH);
				dirList.add(Direction.WEST);
				dirList.add(Direction.NOTHING);
	            this.manager.initialization();
	            for(Island isle:game.islands()){
	            	islandLocations.add(isle.getLocation());
	            }
	            this.inited = true;
	        }
	        if(!myEnemy.isAllDeadEnemy()) {
	        	myEnemy.updateEnemy();
	        }
	        else {
	        	myEnemy.ifAllEnemiesAreDead();
	        }
	        if(!isAllDead()) {
	        	this.updateGameMode();
	        	gameTargetBank.updateTargetBank();
	        	this.manager.updateNavies();
	        	game.debug("GameMode "+gameMode.toString());
	        	this.manager.play(); 
	        }
//		}
	}
	private void oneIslandTactic() {
		if(game.myPirates()!=null && game.myPirates().size()>0) {
			Location myTarget=new Location(0,0);
			int i=-2;
			int additon=0;
			if(game.getCols()/2>game.myPirates().get(0).getLocation().col) {
				additon=-5;
			}
			else {
				additon=5;
			}
		
			for(Pirate myPirate : game.allMyPirates()) {
					myTarget=new Location(game.islands().get(0).getLocation().row+i, game.islands().get(0).getLocation().col+additon);
					if(game.islands().get(0).getTeamCapturing()==Constants.ENEMY || game.enemyPirates().size()<game.myPirates().size()-1) {
						myTarget=game.islands().get(0).getLocation();
						myTarget=new Location(game.islands().get(0).getLocation().row+i, game.islands().get(0).getLocation().col);
					}
					Direction dir=game.getDirections(myPirate, myTarget).get(0);
					game.setSail(myPirate, dir);
					i++;
			}
		}
	}
	private void initialization() {
		if(gameTargetBank==null) {
			gameTargetBank=new TargetBank();
		}
		if (this.manager == null) {
            this.manager = new Manager();
        }
		if(myEnemy==null)
		{
			myEnemy=new Enemy();
		}
		if(this.sailor==null) {
			sailor=new Sailing();
		}
	}
	public boolean isAllDead() {
		int count=0;
		for(Pirate Eldar: game.allMyPirates()) {
			if(Eldar.isLost()) {
				count++;
			}
		}
		return count==game.allMyPirates().size();
	}
	public static int distanceSquare(Location loc1, Location loc2) {
		return (loc1.col-loc2.col)*(loc1.col-loc2.col)+(loc1.row-loc2.row)*(loc1.row-loc2.row);
	}
	public static boolean aboutToWin() {
		//TODO : adding number of turns remaining
		int myScore=game.getMyScore();
		int enemyScore=game.getEnemyScore();
		int myPointsEachTurn=game.getLastTurnPoints()[0];
		int enemyPointsEachTurn=game.getLastTurnPoints()[1];
		int myRemainingPoints=game.getMaxPoints()-myScore;
		int enemyRemainingPoints=game.getMaxPoints()-enemyScore;
		int turnsRemaining=game.getMaxTurns()-game.getTurn();
		int myTurnsToWin;
		int enemyTurnsToWin;
		if(myScore>enemyScore) {
			return true;
			//TODO : check in which conditions this won't be true (even tough we have more points we will lose if this situation is kept)
		}
		if(myPointsEachTurn!=0 && enemyPointsEachTurn!=0) {
			myTurnsToWin=myRemainingPoints/myPointsEachTurn;
			enemyTurnsToWin=enemyRemainingPoints/enemyPointsEachTurn;
			if(myTurnsToWin<enemyTurnsToWin && myTurnsToWin<turnsRemaining) {
				return true;
			}
			else {
				return false;
			}
		}
		else {
			return myRemainingPoints<enemyRemainingPoints;
		}
//		return ((myScore>enemyScore) && (myPointsEachTurn>enemyPointsEachTurn));
	}
	public void updateGameMode() {
		if(aboutToWin()) {
			gameMode=GameMode.ADVANTAGE;
		}
		else {
			gameMode=GameMode.DISADVANTAGE;
		}
	}
	public static boolean inRange(Location loc1, Location loc2) {
		return distanceSquare(loc1, loc2)<=10;
	}
	public static boolean isOnIsland(Location loc){
		return islandLocations.contains(loc);
	}
//	public int[] bestConfiguration()
//    {
//        int[] array;
//        int a=game.islands().size();
//        array = new int[a];
//        for (int i = 0; i < a - 1; i++)
//        {
//            array[i] = 1;
//        }
//        array[a - 1] = this.pirateNumber - (a - 1);
//        return array;
//    }
}
