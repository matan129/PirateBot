package bots;

public enum Priority {
	
	HIGHEST(5), HIGH(4), NORNAL(3), LOW(2), LOWEST(1), WORST(0);
	
	private int value;
	
	private Priority(int value) {
		this.value=value;
	}
	
	public int getValue() {
		return this.value;
	}
	
	public Priority getPriorityByValue(int value) {
		switch (value) {
		case 5:
			return HIGHEST;
		case 4:
			return HIGH;
		case 3:
			return NORNAL;
		case 2:
			return LOW;
		case 1:
			return LOWEST;
		case 0:
			return WORST;
		default:
			return NORNAL;
		}
	}
	public static int getNumberOfPossiblePriority () {
		return Priority.values().length;
	}
}
