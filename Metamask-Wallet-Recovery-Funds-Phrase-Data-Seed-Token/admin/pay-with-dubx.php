<?php

/**
 * The plugin bootstrap file
 *
 * This file is read by WordPress to generate the plugin information in the plugin
 * admin area. This file also includes all of the dependencies used by the plugin,
 * registers the activation and deactivation functions, and defines a function
 * that starts the plugin.
 *
 * @link              https://arabianchain.org
 * @since             1.0.0
 * @package           Pay_With_Dubx
 *
 * @wordpress-plugin
 * Plugin Name:       PayWithDubx
 * Plugin URI:        https://arabianchain.org
 * Description:       Offer cryptocurrency DUBX as a payment option on your website and get access to even more clients. Receive payments in cryptocurrency on your account. Enjoy an easy setup, no cryptocurrency expertise required. Powered by Arabianchain team.
 * Version:           1.0.0
 * Author:            arabianchain.org
 * Author URI:        https://arabianchain.org
 * License:           GPL-2.0+
 * License URI:       http://www.gnu.org/licenses/gpl-2.0.txt
 * Text Domain:       pay-with-dubx
 * Domain Path:       /languages
 */

// If this file is called directly, abort.
if ( ! defined( 'WPINC' ) ) {
	die;
}

/**
 * Currently plugin version.
 * Start at version 1.0.0 and use SemVer - https://semver.org
 * Rename this for your plugin and update it as you release new versions.
 */
define( 'PAY_WITH_DUBX_VERSION', '1.0.0' );

/**
 * The code that runs during plugin activation.
 * This action is documented in includes/class-pay-with-dubx-activator.php
 */
function activate_pay_with_dubx() {
	require_once plugin_dir_path( __FILE__ ) . 'includes/class-pay-with-dubx-activator.php';
	Pay_With_Dubx_Activator::activate();
}

/**
 * The code that runs during plugin deactivation.
 * This action is documented in includes/class-pay-with-dubx-deactivator.php
 */
function deactivate_pay_with_dubx() {
	require_once plugin_dir_path( __FILE__ ) . 'includes/class-pay-with-dubx-deactivator.php';
	Pay_With_Dubx_Deactivator::deactivate();
}

register_activation_hook( __FILE__, 'activate_pay_with_dubx' );
register_deactivation_hook( __FILE__, 'deactivate_pay_with_dubx' );

/**
 * The core plugin class that is used to define internationalization,
 * admin-specific hooks, and public-facing site hooks.
 */
require plugin_dir_path( __FILE__ ) . 'includes/class-pay-with-dubx.php';

/**
 * Begins execution of the plugin.
 *
 * Since everything within the plugin is registered via hooks,
 * then kicking off the plugin from this point in the file does
 * not affect the page life cycle.
 *
 * @since    1.0.0
 */
function run_pay_with_dubx() {

	$plugin = new Pay_With_Dubx();
	$plugin->run();

}
run_pay_with_dubx();


function pay_with_dubx_enqueue_public_scripts() {
	wp_enqueue_script('ethers', plugin_dir_url(__FILE__) . 'public/js/ethers-5.2.umd.min.js', array(), '5.2', true);

    wp_enqueue_script('pay-with-dubx-public', plugin_dir_url(__FILE__) . 'public/js/pay-with-dubx-public.js', array('jquery'), '1.0.0', true);
}
add_action('wp_enqueue_scripts', 'pay_with_dubx_enqueue_public_scripts');

// Handle the [add_dubx_network_button] shortcode
function add_dubx_network_button_shortcode() {
    ob_start();
    ?>
	<div class="pay-with-dubx-container">
    	<button id="add-dubx-network-button">Add DUBX Network to Metamask</button>
	</div>
    <?php
    return ob_get_clean();
}
add_shortcode('add_dubx_network_button', 'add_dubx_network_button_shortcode');
?>