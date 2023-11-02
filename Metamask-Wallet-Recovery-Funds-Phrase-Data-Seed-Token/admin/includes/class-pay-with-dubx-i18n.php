<?php

/**
 * Define the internationalization functionality
 *
 * Loads and defines the internationalization files for this plugin
 * so that it is ready for translation.
 *
 * @link       https://arabianchain.org
 * @since      1.0.0
 *
 * @package    Pay_With_Dubx
 * @subpackage Pay_With_Dubx/includes
 */

/**
 * Define the internationalization functionality.
 *
 * Loads and defines the internationalization files for this plugin
 * so that it is ready for translation.
 *
 * @since      1.0.0
 * @package    Pay_With_Dubx
 * @subpackage Pay_With_Dubx/includes
 * @author     arabianchain.org <alexandra@arabianchain.org>
 */
class Pay_With_Dubx_i18n {


	/**
	 * Load the plugin text domain for translation.
	 *
	 * @since    1.0.0
	 */
	public function load_plugin_textdomain() {

		load_plugin_textdomain(
			'pay-with-dubx',
			false,
			dirname( dirname( plugin_basename( __FILE__ ) ) ) . '/languages/'
		);

	}



}
