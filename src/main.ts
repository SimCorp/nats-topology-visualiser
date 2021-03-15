import Vue from 'vue'

import router from './router'
import App from './App.vue'

import BootstrapVue from 'bootstrap-vue'

import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'

Vue.config.productionTip = false

Vue.use(BootstrapVue)

new Vue({
  router,
  render: createElement => createElement(
    App
  )
}).$mount('#app')
