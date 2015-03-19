(function($) {
	'use strict';
	/**
	 * Teaser module implementation.
	 *
	 * @author Christoph Buehler <christoph.buehler@namics.com>
	 * @namespace Tc.Module
	 * @class Teaser
	 * @extends Tc.Module
	 */
	Tc.Module.Teaser = Tc.Module.extend({

		init: function($ctx, sandbox, modId) {
			this._super($ctx, sandbox, modId);

		},

		on: function(callback) {
			var mod = this,
				$ctx = mod.$ctx;


			callback();
		},

		after: function() {
			var mod = this,
				$ctx = mod.$ctx;


		}
	});
}(Tc.$));
