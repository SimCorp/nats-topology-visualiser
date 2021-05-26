import Vue from 'vue'
import router from './router'
import App from './App.vue'
import JSONView from 'vue-json-component'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'
import BootstrapVue from 'bootstrap-vue'
// @ts-ignore
import VueZoomer from 'vue-zoomer'
// @ts-ignore
import VueTreeList from 'vue-tree-list'

Vue.config.productionTip = false

Vue.use(VueTreeList)
Vue.use(BootstrapVue)
Vue.use(VueZoomer)
Vue.use(JSONView)

new Vue({
  router,
  render: createElement => createElement(
    App
  )
}).$mount('#app')
