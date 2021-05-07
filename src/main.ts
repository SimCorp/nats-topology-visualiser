import Vue from 'vue'

import router from './router'
import App from './App.vue'

import BootstrapVue from 'bootstrap-vue'

// @ts-ignore
import VueZoomer from 'vue-zoomer'
// @ts-ignore
import Panzoom from 'vue-panzoom'

import JSONView from 'vue-json-component'

import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'

Vue.config.productionTip = false

Vue.use(BootstrapVue)
Vue.use(VueZoomer)
Vue.use(JSONView)

new Vue({
  router,
  render: createElement => createElement(
    App
  )
}).$mount('#app')
