package bots;

import java.util.*;

import pirates.game.*;

public class Target {
	
	private Object item;
	private int id;
	private Priority priority;
	private int requirements;
	private PirateGame game=MyBot.game;
	
	public Target(Object item) {
		this.item=item;
		if(this.getType().equals(TargetUtilities.ISLAND)) {
			Island myIsland=(Island)this.item;
			this.id=myIsland.getId();
			if(MyBot.gameMode==GameMode.DISADVANTAGE) {
				this.priority=Priority.NORNAL;
			}
			if(MyBot.gameMode==GameMode.ADVANTAGE) {
				this.priority=Priority.LOW;
			}
		}
		if (this.getType().equals(TargetUtilities.ENEMY_PIRATE)) {
			Pirate enemyPirate=(Pirate)this.item;
			this.id=enemyPirate.getId();
			if(MyBot.gameMode==GameMode.DISADVANTAGE) {
				this.priority=Priority.LOW;
			}
			if(MyBot.gameMode==GameMode.ADVANTAGE) {
				this.priority=Priority.NORNAL;
			}
		}
		if(this.getType().equals(TargetUtilities.ENEMY_NAVY)) {
			EnemyNavy eNavy=(EnemyNavy)this.getObject();
			this.id=eNavy.getID();
			if(MyBot.gameMode==GameMode.DISADVANTAGE) {
				this.priority=Priority.LOW;
			}
			if(MyBot.gameMode==GameMode.ADVANTAGE) {
				this.priority=Priority.NORNAL;
			}
		}
		if(this.getType().equals(TargetUtilities.ASSIST)) {
			this.priority=Priority.HIGHEST;
		}
	}
	
	public Target() {
		this.priority=Priority.LOWEST;
	}
	
	public void setPriority(Priority targetPriority) {
		this.priority=targetPriority;
	}
	
	public void changePriority(int change) {
		int maxPriority=5;
		switch (MyBot.gameMode) {
		case DISADVANTAGE:
			if (!this.getType().equals(TargetUtilities.ISLAND)) {
				maxPriority=2;
			}
			break;
		default:
			maxPriority=5;
			break;
		}
		int newValue=this.priority.getValue()+change;
		if(newValue>maxPriority) {
			this.priority=priority.getPriorityByValue(5);
		}
		if(newValue<0) {
			this.priority=priority.getPriorityByValue(0);
		}
		if(newValue>=0 && newValue<=maxPriority) {
			this.priority=priority.getPriorityByValue(newValue);
		}
	}
	
	public void setRequirements (int requirements) {
		this.requirements=requirements;
	}
	public int getRequirements () {
		return this.requirements;
	}
	
	public Object getObject() {
		return this.item;
	}
	public String getType() {
		String typeString;
		if(this.item==null) {
			typeString="null";
		}
		else{
			typeString=this.item.toString();			
		}
		if(typeString.contains("Island")) {
			return TargetUtilities.ISLAND;
		}
		if(typeString.contains("Pirate")) {
			return TargetUtilities.ENEMY_PIRATE;
		}
		if(typeString.contains("EnemyNavy")) {
			return TargetUtilities.ENEMY_NAVY;
		}
		if(typeString.contains("Navy")) {
			return TargetUtilities.ASSIST;
		}
		return typeString;
	}
	public int getID() {
		return this.id;
	}
	public Priority getPriority() {
		return this.priority;
	}
	
	public Location getLocation() {
		Location defultLocation=new Location(0, 0);
		if(this.getType().equals(TargetUtilities.ISLAND)) {
			Island isle=game.getIsland(this.id);
			return isle.getLocation();
		}
		if(this.getType().equals(TargetUtilities.ENEMY_PIRATE)) {
			Pirate pir=game.getEnemyPirate(this.id);
			return pir.getLocation();
		}
		if(this.getType().equals(TargetUtilities.ENEMY_NAVY)) {
			EnemyNavy eNavy=(EnemyNavy)this.getObject();
			return eNavy.getLoc();
		}
		if(this.getType().equals(TargetUtilities.ASSIST)) {
			Navy myNavy=(Navy)this.getObject();
			return myNavy.getLocation();
		}
		return defultLocation;
	}
	
	@Override
	public String toString() {
		String str="Target Type : "+this.getType();
		str+="\nTarget's ID : "+this.id+";";
		str+="\nPriority : "+this.priority+"("+this.priority.getValue()+")"+";";
		str+="\nLocation : "+this.getLocation();
		return str;
	}
	
}
