/**
 * @class Base class for maps
 * @extends CanvasElement
 * @constructor
 * @param {State}
 *        state the visualizer state for reference
 */
function CanvasElementAbstractMap(state) {
	this.upper();
	this.state = state;
	this.water = null;
}
CanvasElementAbstractMap.extend(CanvasElement);

/**
 * Draws a red marker on the map. Used when coordinates are given in the replay URL.
 *
 * @param {Number}
 *        xs the x pixel position
 * @param {Number}
 *        ys the y pixel position
 */
CanvasElementAbstractMap.prototype.redFocusRectFun = function(xs, ys) {
	var x, y, w, i;
	for (i = 0; i < 5; i++) {
		this.ctx.strokeStyle = 'rgba(255,0,0,' + (i + 1) / 5 + ')';
		w = this.scale + 9 - 2 * i;
		x = xs + i;
		y = ys + i;
		this.ctx.strokeRect(x, y, w, w);
	}
};

/**
 * Draws the terrain map.
 */
CanvasElementAbstractMap.prototype.draw = function() {
	var row, col, xs, ys;
	var rows = this.state.replay.rows;
	var cols = this.state.replay.cols;
	var rowOpt = this.state.options['row'];
	var colOpt = this.state.options['col'];
	this.ctx.fillStyle = this.ctx.createPattern(this.water, 'repeat');
	this.ctx.fillRect(0, 0, this.w, this.h);

	// marker
	if (!isNaN(rowOpt) && !isNaN(colOpt)) {
		xs = (colOpt % cols) * this.scale - 4.5;
		ys = (rowOpt % rows) * this.scale - 4.5;
		this.drawWrapped(xs, ys, this.scale + 9, this.scale + 9, this.w, this.h,
				this.redFocusRectFun, [ xs, ys ]);
	}
};